using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using MatrixBook.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using IO = System.IO;

namespace MatrixBook.Server.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class VideoController(
     IOptions<AppConfiguration> configuration,
     IEntityRepository entityRepository,
     IBookImageCache bookImageCache,
     IVideoExportService videoExportService,
     IBookExportService bookExportService,
     ILogger<VideoController> logger) : ControllerBase
{
    private readonly ILogger<VideoController> _logger = logger;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IBookImageCache _bookImageCache = bookImageCache;
    private readonly IVideoExportService _videoExportService = videoExportService;
    private readonly IBookExportService _bookExportService = bookExportService;
    private readonly AppConfiguration _configuration = configuration.Value;

    [HttpGet("{page?}/{pageSize?}", Name = "getAllVideoInfo")]
    public async Task<IEnumerable<VideoMeta>> GetAll(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var videos = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<VideoMeta>().OrderByDescending(x => x.DateUpdated);

            return query;
        });

        var pagedVideos = videos
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return pagedVideos;
    }

    [HttpGet("search", Name = "searchVideos")]
    public async Task<IEnumerable<VideoMeta>> Search([FromQuery] string keyword = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var videos = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<VideoMeta>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword).Search(x => x.Content, keyword);

            }
            return query;
        });

        var pagedVideos = videos
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return pagedVideos;
    }

    [HttpGet("{id}", Name = "getVideoById")]
    public async Task<IActionResult> Get(string id)
    {
        var video = await _entityRepository.GetByIdAsync<VideoMeta>(id);
        if (video == null)
            return NotFound();
        return Ok(video);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VideoMeta videoMeta)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var now = DateTime.Now;
        if (!videoMeta.DateCreated.HasValue)
            videoMeta.DateCreated = now;
        videoMeta.DateUpdated = now;
        await _entityRepository.UpdateAsync(videoMeta);
        return Ok(videoMeta);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] VideoMeta videoMeta)
    {
        if (id != videoMeta.Id)
            return BadRequest();

        var existing = await _entityRepository.GetByIdAsync<VideoMeta>(id);
        if (existing == null)
            return NotFound();

        var now = DateTime.Now;
        if (!videoMeta.DateCreated.HasValue)
            videoMeta.DateCreated = now;
        videoMeta.DateUpdated = now;

        await _entityRepository.UpdateAsync(videoMeta);
        return Ok(videoMeta);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var video = await _entityRepository.GetByIdAsync<VideoMeta>(id);
        if (video == null)
            return Ok(false);

        await _entityRepository.DeleteAsync<VideoMeta>(id);
        return Ok(true);
    }

    [HttpPost("export", Name = "exportVideo")]
    public async Task<IActionResult> Export([FromBody] VideoMeta videoMeta)
    {
        await _videoExportService.Export(videoMeta);
        return Ok();
    }

    [HttpPost("export/video", Name = "exportVideoOnly")]
    public async Task<IActionResult> ExportVideo([FromBody] VideoMeta videoMeta)
    {
        await _videoExportService.ExportVideoOnly(videoMeta);
        return Ok();
    }

    [HttpPost("openVideoFolder", Name = "openVideoFolder")]
    public async Task<IActionResult> OpenVideoFolder([FromBody] VideoMeta videoMeta)
    {
        var videoPath = Path.Combine(_configuration.VideoRootFolder, ResourceTypes.Output);

        if (videoMeta != null && videoMeta.Category.IsNotNullOrEmpty())
        {
            videoPath = videoMeta.GetOutputFileName(_configuration.VideoRootFolder);
        }

        await _bookExportService.SelectFile(videoPath);
        return Ok();
    }

    [HttpGet("media/resources", Name = "getAllMediaResources")]
    public async Task<IEnumerable<MediaResource>> GetAllMediaResources()
    {
        var resources = await _entityRepository.GetAllAsync<MediaResource>();
        return resources;
    }

    [HttpGet("media/resources/{id}", Name = "getMediaResourceById")]
    public async Task<IActionResult> GetMediaResource(string id)
    {
        var resource = await _entityRepository.GetByIdAsync<MediaResource>(id);
        if (resource == null)
            return NotFound();
        return Ok(resource);
    }

    [HttpGet("media/resources/url/{id}", Name = "getMediaResourceUrl")]
    public async Task<IActionResult> GetMediaResourceUrl(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest("URL cannot be empty.");

        var (exists, imagePath) = _bookImageCache.GetImagePath(id);
        if (exists)
        {
            return PhysicalFile(imagePath, "image/jpg");
        }

        var etag = $"\"{id.GetHashCode()}\"";
        if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
        {
            return StatusCode(304); // Not Modified
        }

        var resource = await _entityRepository.GetByIdAsync<MediaResource>(id);

        if (resource != null)
        {
            var fullImagePath = Path.Combine(_configuration.VideoRootFolder, ResourceTypes.ResourceRoot, resource.Url);
            if (IO.File.Exists(fullImagePath))
            {
                _bookImageCache.SetImagePath(id, fullImagePath);

                var extension = Path.GetExtension(fullImagePath);
                var clientFilename = $"{id}{extension}";
                Response.Headers.CacheControl = "public, max-age=31536000";                  // Cache for 1 year
                Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");        // Optional
                Response.Headers.ETag = $"\"{id.GetHashCode()}\"";                      // Optional: Use an ETag for validation

                return PhysicalFile(fullImagePath, "image/jpg", clientFilename);
            }
        }

        return NotFound();
    }

    [HttpGet("media/resources/type/{type}", Name = "getMediaResourcesByType")]
    public async Task<IEnumerable<MediaResource>> GetMediaResourcesByType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return await _entityRepository.GetAllAsync<MediaResource>();
        var lowerType = type.ToLowerInvariant();
        var resources = await _entityRepository.QueryAsync(session =>
        {
            return session.Query<MediaResource>().Search(x => x.Type, lowerType);
        });
        return resources;
    }

    [HttpPost("media/resources")]
    public async Task<IActionResult> CreateMediaResource([FromBody] MediaResource mediaResource)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _entityRepository.UpdateAsync(mediaResource);
        return Ok(mediaResource);
    }

    [HttpPut("media/resources/{id}")]
    public async Task<IActionResult> UpdateMediaResource(string id, [FromBody] MediaResource mediaResource)
    {
        if (id != mediaResource.Id)
            return BadRequest();

        var existing = await _entityRepository.GetByIdAsync<MediaResource>(id);
        if (existing == null)
            return NotFound();

        await _entityRepository.UpdateAsync(mediaResource);
        return Ok(mediaResource);
    }

    [HttpDelete("media/resources/{id}")]
    public async Task<IActionResult> DeleteMediaResource(string id)
    {
        var resource = await _entityRepository.GetByIdAsync<MediaResource>(id);
        if (resource == null)
            return Ok(false);

        await _entityRepository.DeleteAsync<MediaResource>(id);
        return Ok(true);
    }

    [HttpGet("media/scan")]
    public async Task<IActionResult> Scan()
    {
        var resourcePath = Path.Combine(_configuration.VideoRootFolder, ResourceTypes.ResourceRoot);

        if (!Directory.Exists(resourcePath))
            return NotFound($"Resource path not found: {resourcePath}");

        var jpgFiles = Directory.GetFiles(resourcePath, $"*{ResourceTypes.SplashType}", SearchOption.AllDirectories);
        var pngFiles = Directory.GetFiles(resourcePath, $"*{ResourceTypes.PngType}", SearchOption.AllDirectories);
        var mp3Files = Directory.GetFiles(resourcePath, $"*{ResourceTypes.AudioType}", SearchOption.AllDirectories);

        await EnumerateResourceFiles(jpgFiles, ResourceTypes.Image, resourcePath);
        await EnumerateResourceFiles(pngFiles, ResourceTypes.Image, resourcePath);
        await EnumerateResourceFiles(mp3Files, ResourceTypes.Audio, resourcePath);

        return Ok();
    }

    [HttpPost("init")]
    public async Task<IActionResult> Init()
    {
        var fakeVideos = new List<VideoMeta>
        {

        };

        var fakeResources = new List<MediaResource>
        {
            new() {
                Id = Guid.NewGuid().ToString(),
                Name = "Matrix Trailer",
                Url = "https://example.com/matrix-trailer"
            },
            new() {
                Id = Guid.NewGuid().ToString(),
                Name = "Behind the Scenes",
                Url = "https://example.com/behind-the-scenes"
            }
        };

        foreach (var video in fakeVideos)
        {
            await _entityRepository.UpdateAsync(video);
        }

        foreach (var resource in fakeResources)
        {
            await _entityRepository.UpdateAsync(resource);
        }

        return Ok(new { Videos = fakeVideos.Count, Resources = fakeResources.Count });
    }


    [HttpPost("media/update/images")]
    public async Task<IActionResult> UpdateImages()
    {
        var resourcePath = Path.Combine(_configuration.VideoRootFolder, ResourceTypes.ResourceRoot);
        var jpgFiles = Directory.GetFiles(resourcePath, $"*{ResourceTypes.SplashType}", SearchOption.AllDirectories);

        foreach (var file in jpgFiles)
        {
            var fileName = Path.GetFileName(file);
            var folder = file[(resourcePath.Length + 1)..];

            var resource = await _entityRepository.GetResourceByName(fileName);
            if (resource != null && resource.Url != folder)
            {
                resource.Url = folder;
                await _entityRepository.UpdateAsync(resource);
            }
            else
            {
                resource = new MediaResource
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = fileName,
                    Url = folder,
                    Type = ResourceTypes.Image.ToLower()
                };
                await _entityRepository.UpdateAsync(resource);
            }
        }

        return Ok();
    }

    [HttpGet("media/resource/groups/{type}", Name = "getResourceGroups")]
    public async Task<IActionResult> GetResourceGroupNames(string type)
    {
        var lowerType = type.ToLowerInvariant();
        var resources = await _entityRepository.QueryAsync(session =>
        {
            return session.Query<MediaResource>().Search(x => x.Type, lowerType);
        });

        var groups = new List<MediaResourceGroup>();

        foreach (var resource in resources)
        {
            var parts = resource.Url.Split('\\');

            foreach (var part in parts.Where(x => x.ToLower() != lowerType
                                        && !(x.EndsWith(ResourceTypes.SplashType) || x.EndsWith(ResourceTypes.PngType)))
                )
            {
                var group = groups.FirstOrDefault(g => g.Name == part);
                if (group == null)
                {
                    group = new MediaResourceGroup { Name = part, Resources = new List<MediaResource>() };
                    groups.Add(group);
                }
                group.Resources.Add(resource);
            }
        }

        return Ok(groups);
    }

    private async Task EnumerateResourceFiles(IEnumerable<string> files, string resourceType, string rootFolder)
    {
        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var folder = file[(rootFolder.Length + 1)..];

            bool exists = await _entityRepository.ExistsAsync<MediaResource>(session =>
            {
                return session.Query<MediaResource>().Where(x => x.Url == folder);
            });

            if (exists)
                continue;

            var resource = new MediaResource
            {
                Id = Guid.NewGuid().ToString(),
                Name = fileName,
                Url = folder,
                Type = resourceType.ToLower()
            };
            await _entityRepository.UpdateAsync(resource);
        }
    }
}
