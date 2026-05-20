using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Extensions;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using IO = System.IO;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ImageController(
    IOptions<AppConfiguration> appConfiguration,
    IEntityRepository entityRepository,
    IHostEnvironment hostEnvironment,
    ICacheService cacheService,
    ICacheKeyService cacheKeyService,
    IMemoryCache memoryCache,
    ILogger<ImageController> logger
    ) : ControllerBase
{
    private readonly AppConfiguration _appConfiguration = appConfiguration.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ICacheKeyService _cacheKeyService = cacheKeyService;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ILogger<ImageController> _logger = logger;

    private const string ImageContentType = "image/jpg";


    [HttpGet("{id}", Name = "getImage")]
    //[Cache]
    public async Task<IActionResult> Get(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            _logger.LogInformation("Getting image for id: {}", id);
            var key = _cacheKeyService.GetRequestKey(HttpContext);
            var filePath = _memoryCache.Get<string>(key);

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = await GetImagePath(id);
            }

            var etag = $"\"{id.GetHashCode()}\"";
            if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
            {
                return StatusCode(304);
            }

            if (IO.File.Exists(filePath))
            {
                await _cacheService.SetAsync(key, filePath, CacheSettings.CacheDurationInMinuteLong);

                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                var extension = Path.GetExtension(filePath);
                var fileName = $"{id}{extension}";
                //Response.Headers.CacheControl = "public, max-age=31536000";              // Cache for 1 year
                //Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");    // Optional
                //Response.Headers.ETag = $"\"{id.GetHashCode()}\"";                       // Optional: Use an ETag for validation

                return File(fileStream, ImageContentType, fileName);
            }
            else
            {
                _logger.LogWarning("File not found: {}", filePath);
                return NotFound();
            }
        }

        return NotFound();
    }

    [HttpGet("avatars", Name = "getAvatars")]
    public Task<IActionResult> GetAvatars()
    {
        var path = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "assets", "images", "avatars");
        if (!Directory.Exists(path))
        {
            _logger.LogWarning("Directory does not exist: {}", path);
            return Task.FromResult<IActionResult>(NotFound());
        }

        var key = _cacheKeyService.GetRequestKey(HttpContext);
        var cachedFiles = _cacheService.GetAsync<IEnumerable<string>>(key);
        if (cachedFiles != null)
        {
            return Task.FromResult<IActionResult>(Ok(cachedFiles));
        }

        var files = Directory.GetFiles(path).Select(x => Path.GetFileName(x));
        _cacheService.SetAsync(key, files, CacheSettings.CacheDurationInMinuteLong);

        return Task.FromResult<IActionResult>(Ok(files));
    }

    [HttpPost("reset/album/splashes", Name = "resetAlbumSplashes")]
    [Authorize]
    public async Task<IActionResult> ResetAlbumSplashes()
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>());
        foreach (var album in albums)
        {
            album.ImageWideSplashId = null;
            album.ImageSquareSplashId = null;
            await _entityRepository.UpdateAsync(album);
        }
        return Ok(new ResultBase { Success = true, Message = "Splashes are reset" });
    }

    [HttpPost("reset/artist/avatars", Name = "resetArtistAvatars")]
    [Authorize]
    public async Task<IActionResult> ResetArtistAvatars()
    {
        var artists = await _entityRepository.QueryAsync(session => session.Query<Artist>());
        foreach (var artist in artists)
        {
            artist.Image = null;
            await _entityRepository.UpdateAsync(artist);
        }
        return Ok(new ResultBase { Success = true, Message = "Avatars are reset" });
    }

    [HttpPost("fix/album/splashes", Name = "fixSplashes")]
    [Authorize]
    public async Task<IActionResult> FixSplashes()
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                          .Where(x => string.IsNullOrEmpty(x.ImageWideSplashId) || string.IsNullOrEmpty(x.ImageSquareSplashId)));
        foreach (var album in albums)
        {
            var imageFolder = Path.Combine(_appConfiguration.BooksLocation, album.Artist!, album.Title!, "images");

            var wideBg = Path.Combine(imageFolder, $"{album.Title}-wide-bg.jpg");
            var squareBg = Path.Combine(imageFolder, $"{album.Title}-square-bg.jpg");

            _logger.LogInformation("Checking album {}, {}, {}", album.Title, wideBg, squareBg);

            if (IO.File.Exists(wideBg))
            {
                album.ImageWideSplashId = Guid.NewGuid().ToString();
                var imageResource = new ImageResource
                {
                    Id = album.ImageWideSplashId,
                    FolderName = @$"{album.Artist}\{album.Title}",
                    FileName = $"{album.Title}-wide-bg.jpg"
                };
                await _entityRepository.UpdateAsync(imageResource);

                _logger.LogInformation("Wide splashes are update for album: {}", album.Title);
            }

            if (IO.File.Exists(squareBg))
            {
                album.ImageSquareSplashId = Guid.NewGuid().ToString();
                var imageResource = new ImageResource
                {
                    Id = album.ImageSquareSplashId,
                    FolderName = @$"{album.Artist}\{album.Title}",
                    FileName = $"{album.Title}-square-bg.jpg"
                };
                await _entityRepository.UpdateAsync(imageResource);

                _logger.LogInformation("Square splashes are update for album: {}", album.Title);
            }
            await _entityRepository.UpdateAsync(album);
        }
        return Ok(new ResultBase { Success = true, Message = "Splashes are fixed" });
    }

    [HttpPost("fix/artist/avatars", Name = "fixArtistAvatars")]
    [Authorize]
    public async Task<IActionResult> FixArtistAvatars()
    {
        var artists = await _entityRepository.QueryAsync(session => session.Query<Artist>()
                                                                           .Where(x => string.IsNullOrEmpty(x.Image)));

        foreach (var artist in artists)
        {
            var imageFolder = Path.Combine(_appConfiguration.BooksLocation, artist.Name!, "images");
            imageFolder.EnsureDirectoryExists();

            var avatar = Path.Combine(imageFolder, $"{artist.Name}.jpg");

            _logger.LogInformation("Checking artist {}, {}", artist.Name, avatar);
            if (IO.File.Exists(avatar))
            {
                artist.Image = Guid.NewGuid().ToString();
                var imageResource = new ImageResource
                {
                    Id = artist.Image,
                    FolderName = @$"{artist.Name}",
                    FileName = $"{artist.Name}.jpg"
                };
                await _entityRepository.UpdateAsync(imageResource);
                _logger.LogInformation("Artist avatars are updated for artist: {}", artist.Name);
            }
            await _entityRepository.UpdateAsync(artist);
        }
        return Ok(new ResultBase { Success = true, Message = "Artist avatars are fixed" });
    }

    [HttpPost("post", Name = "uploadPostImage")]
    [Authorize]
    public async Task<IActionResult> UploadPostImage(IFormFile file)
    {
        if (file == null || file.Length == 0 || !IsSupportedImage(file.FileName))
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            var postFolder = "post";
            var postImageFolder = Path.Combine(_appConfiguration.BooksLocation, postFolder, "images");
            postImageFolder.EnsureDirectoryExists();

            var filePath = Path.Combine(postImageFolder, file.FileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var imageResource = new ImageResource
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName,
                FolderName = postFolder,
            };
            await _entityRepository.UpdateAsync(imageResource);

            return Ok(imageResource);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}", Name = "deleteImage")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var imageResource = await _entityRepository.GetAsync<ImageResource>(id);
        if (imageResource != null)
        {
            var folderName = imageResource.FolderName!.Replace('\\', Path.DirectorySeparatorChar) ?? string.Empty;
            var file = Path.Combine(_appConfiguration.BooksLocation, folderName!, "images", imageResource.FileName!);
            if (IO.File.Exists(file))
            {
                IO.File.Delete(file);
            }
            await _entityRepository.DeleteAsync<ImageResource>(id);
        }

        return Ok(true);
    }

    [HttpPost("convert/png/to/jpg", Name = "convertPngToJpb")]
    public async Task<IActionResult> ConvertPngToJpb()
    {
        var images = await _entityRepository.GetAllAsync<ImageResource>();
        foreach (var image in images)
        {
            if (image.FileName?.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == true)
            {
                image.FileName = $"{Path.GetFileNameWithoutExtension(image.FileName)}.jpg";
                await _entityRepository.UpdateAsync(image);
            }
        }
        return Ok();
    }


    private async Task<string> GetImagePath(string id)
    {
        var imageResource = await _entityRepository.GetAsync<ImageResource>(id);
        if (imageResource == null)
        {
            return string.Empty;
        }

        var folderName = imageResource.FolderName!.Replace('\\', Path.DirectorySeparatorChar) ?? string.Empty;
        var file = Path.Combine(_appConfiguration.BooksLocation, folderName!, "images", imageResource.FileName!);
        return file;
    }

    private static bool IsSupportedImage(string fileName)
    {
        var supportedExtensions = new HashSet<string>() { ".jpg", ".jpeg", ".png" };
        return !string.IsNullOrEmpty(fileName)
            && supportedExtensions.Contains(Path.GetExtension(fileName).ToLower());
    }
}
