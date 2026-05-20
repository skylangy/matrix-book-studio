using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AlbumController(
    IOptions<AppConfiguration> appConfiguration,
    IEntityRepository entityRepository,
    ICacheService cacheService,
    ICacheKeyService cacheKeyService,
    IMetadataProcessor metadataProcessor,
    ILogger<AlbumController> logger
    ) : ControllerBase
{
    private readonly AppConfiguration _appConfiguration = appConfiguration.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ICacheService _cacheService = cacheService;
    private readonly ICacheKeyService _cacheKeyService = cacheKeyService;
    private readonly IMetadataProcessor _metadataProcessor = metadataProcessor;
    private readonly ILogger<AlbumController> _logger = logger;


    [HttpGet("all/{page}/{count}", Name = "allAlbums")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAll(int page = 1, int count = 12)
    {
        _logger.LogInformation("Getting all albums with page {count}", count);

        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                          .OrderByDescending(x => x.DateUpdated)
                                                                          .Skip((page - 1) * count)
                                                                          .Take(count));

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("recents/{page}/{pageSize}", Name = "recentRelease")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetRecents(int page = 1, int pageSize = 12)
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                             .OrderByDescending(x => x.DateUpdated)
                                                                             .Skip((page - 1) * pageSize)
                                                                             .Take(pageSize));

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("suggested/{page}/{count}", Name = "getSuggested")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetSuggested(int page = 1, int count = 12)
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                          .OrderByDescending(x => x.Ranking)
                                                                          .Skip((page - 1) * count)
                                                                          .Take(count));

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("by/likes/{page}/{count}", Name = "getMostLikes")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetMostLikes(int page = 1, int count = 12)
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                          .OrderByDescending(x => x.LikeCount)
                                                                          .Skip((page - 1) * count)
                                                                          .Take(count));

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("by/plays/week/{page}/{count}", Name = "getByPlaysWeek")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPopularsWeek(int page = 1, int count = 12)
    {
        var startDay = DateTime.Now.AddDays(-7);

        var viewModels = await GetMostPlayAlbums(page, count, startDay);
        return Ok(viewModels);
    }

    [HttpGet("by/plays/month/{page}/{count}", Name = "getByPlaysMonth")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPopularsMonth(int page = 1, int count = 12)
    {
        var startDay = DateTime.Now.AddDays(-30);
        var viewModels = await GetMostPlayAlbums(page, count, startDay);
        return Ok(viewModels);
    }

    [HttpGet("by/plays/year/{page}/{count}", Name = "getByPlaysYear")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPopularsYear(int page = 1, int count = 12)
    {
        var startDay = DateTime.Now.AddDays(-365);
        var viewModels = await GetMostPlayAlbums(page, count, startDay);
        return Ok(viewModels);
    }

    [HttpGet("category/{categoryId}/{page}/{count}", Name = "getByCategory")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetByCategory(string categoryId, int page = 1, int count = 12)
    {
        var albums = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Album>();

            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                query = query.Where(x => x.Categories!.Contains(categoryId));
            }

            return query.OrderByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * count)
                        .Take(count);
        });

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("tag/{tagId}/{page}/{count}", Name = "getByTag")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetByTag(string tagId, int page = 1, int count = 12)
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                             .Where(x => x.Tags.Contains(tagId))
                                                                             .OrderByDescending(x => x.DateUpdated)
                                                                             .Skip((page - 1) * count)
                                                                             .Take(count));

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("search", Name = "searchAlbums")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int count = 12)
    {
        _logger.LogInformation("Searching albums with keyword {keyword}, {page}", keyword, page);
        var albums = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Album>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword)
                             .Search(x => x.Subtitle, keyword)
                             .Search(x => x.Description, keyword);
            }

            return query.OrderByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * count)
                        .Take(count);
        });

        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("by/date/{startDate}/{endDate}/{page}/{count}", Name = "getByDate")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetByDate(DateTime startDate, DateTime endDate, int page = 1, int count = 500)
    {
        var albums = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                             .Where(x => x.DateUpdated >= startDate && x.DateUpdated <= endDate)
                                                                             .OrderByDescending(x => x.DateUpdated)
                                                                             .Skip((page - 1) * count)
                                                                             .Take(count));
        var viewModels = await albums.ToViewModels(GetArtist, GetAlbumEpisodeCount);
        return Ok(viewModels);
    }

    [HttpGet("{id}", Name = "getAlbum")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(string id)
    {
        var album = await _entityRepository.GetAsync<Album>(id);
        if (album == null)
        {
            return NotFound();
        }

        var episodes = await _entityRepository.GetAlbumEpisodesAsync(album.Id!);
        foreach (var episode in episodes)
        {
            await UpdateEpisodeFileLength(album, episode);
        }

        var viewModel = album.ToViewModel();
        viewModel.Artist = await GetArtist(album.ArtistId!) ?? new ArtistViewModel();
        viewModel.Episodes = [.. episodes.ToViewModels(album, false)];
        viewModel.EpisodeCount = episodes.Count();
        viewModel.DurationInSeconds = episodes.Sum(x => x.Duration) / 1000;
        return Ok(viewModel);
    }

    [HttpPost("", Name = "createAlbum")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] Album album)
    {
        album.Id = Guid.NewGuid().ToString();
        album.DateCreated = DateTime.Now;
        album.DateUpdated = DateTime.Now;

        var result = await _entityRepository.UpdateAsync(album);
        if (!result)
        {
            return BadRequest();
        }

        return Ok(result);
    }

    [HttpPost("update", Name = "updateAlbum")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] AlbumViewModel viewModel)
    {
        if (viewModel == null)
        {
            return BadRequest();
        }
        var album = await _entityRepository.GetAsync<Album>(viewModel.Id!);
        album.DateUpdated = DateTime.Now;
        album.Title = viewModel.Title;
        album.Description = viewModel.Description;
        album.Level = viewModel.Level;

        var result = await _entityRepository.UpdateAsync(album);
        if (!result)
        {
            return BadRequest();
        }

        return Ok(album.ToViewModel());
    }

    [HttpPost("scan/metadata", Name = "scanAlbumMetadata")]
    [Authorize]
    public async Task<IActionResult> ScanAlbums()
    {
        var count = await _metadataProcessor.ScanAlbumMetadata();
        return Ok(new ResultBase { Success = true, Message = $"{count} albums processed." });
    }

    [HttpDelete("{id}", Name = "deleteAlbum")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _entityRepository.DeleteAsync<Album>(id);
        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpGet("episode/{albumId}/{episodeId}", Name = "getEpisode")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetEpisode(string albumId, string episodeId)
    {
        var album = await _entityRepository.GetAsync<Album>(albumId);
        var episode = await _entityRepository.GetAsync<Episode>(episodeId);
        await UpdateEpisodeFileLength(album, episode);

        var viewModel = episode?.ToViewModel(album);

        return Ok(viewModel);
    }

    [HttpGet("episode/stream/{albumId}/{episodeId}", Name = "getEpisodeStream")]
    public async Task<IActionResult> GetEpisodeStream(string albumId, string episodeId)
    {
        var album = await _entityRepository.GetAsync<Album>(albumId);
        var episode = await _entityRepository.GetAsync<Episode>(episodeId);

        if (episode == null)
        {
            _logger.LogInformation("Episode not found {} {}", albumId, episodeId);
            return NotFound();
        }

        var result = episode.GetEpisodeStreamAsync(_appConfiguration.BooksLocation, album.Artist!, album.Title!, _logger);
        return result;
    }

    [HttpGet("stream/episode/{episodeId}", Name = "streamEpisode")]
    public async Task<IActionResult> StreamEpisode(string episodeId)
    {
        var episode = await _entityRepository.GetAsync<Episode>(episodeId);
        var album = await _entityRepository.GetAsync<Album>(episode.AlbumId);
        if (episode == null)
        {
            _logger.LogInformation("Episode not found {}", episodeId);
            return NotFound();
        }

        var result = episode.GetEpisodeStreamAsync(_appConfiguration.BooksLocation, album.Artist!, album.Title!, _logger);
        return result;
    }

    [HttpGet("episode/url/{albumId}/{episodeId}", Name = "getEpisodeUrl")]
    public async Task<IActionResult> GetEpisodeUrl(string albumId, string episodeId)
    {
        var album = await _entityRepository.GetAsync<Album>(albumId);
        var episode = await _entityRepository.GetAsync<Episode>(episodeId);

        if (episode == null)
        {
            _logger.LogInformation("Episode not found {} {}", albumId, episodeId);
            return NotFound();
        }

        var fileName = $"{episode.Title}.mp3";
        var filePath = Path.Combine(_appConfiguration.BooksLocation, album.Artist!, album.Title!, "mp3", fileName);


        return Ok(new { Url = filePath });
    }

    [HttpGet("categories", Name = "getCategories")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _entityRepository.QueryAsync(session => session.Query<Category>());

        return Ok(categories);
    }

    [HttpGet("tags", Name = "getTags")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetTags()
    {
        var tags = await _entityRepository.QueryAsync(session => session.Query<Tag>());

        return Ok(tags);
    }

    [HttpGet("groups", Name = "getAlbumGroups")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAlbumGroups()
    {
        var categories = await _entityRepository.QueryAsync(session => session.Query<Category>());
        var tags = await _entityRepository.QueryAsync(session => session.Query<Tag>());


        var groups = categories.Select(x => new GroupViewModel { Id = x.Id, Name = x.Name!, Type = nameof(Category) })
                               .Concat(tags.Select(x => new GroupViewModel { Id = x.Id, Name = x.Name!, Type = nameof(Tag) }));

        return Ok(groups);
    }

    private async Task<ArtistViewModel?> GetArtist(string artistId)
    {
        var artist = await _entityRepository.GetArtistViewModel(artistId);
        return artist;
    }

    private async Task<int> GetAlbumEpisodeCount(string albumId)
    {
        var count = await _entityRepository.CountAsync(session => session.Query<Episode>()
                                                                            .Where(x => x.AlbumId == albumId));
        return count;
    }

    private async Task<IEnumerable<AlbumViewModel>> GetMostPlayAlbums(int page, int count, DateTime startDay)
    {
        var albumIds = await _entityRepository.QueryAsync(session => session.Query<UserOperation>()
                                                                               .Where(x => x.Operation == UserOperationType.PlayAlbum && x.DateCreated >= startDay)
                                                                               .OrderByDescending(x => x.Count)
                                                                               .Select(x => x.TargetId)
                                                                               .Distinct()
                                                                               .Skip((page - 1) * count)
                                                                               .Take(count));

        return await _entityRepository.QueryAlbumsByIds(albumIds);
    }

    private async Task UpdateEpisodeFileLength(Album album, Episode episode)
    {
        if (episode.FileLength == 0)
        {
            var filePath = Path.Combine(_appConfiguration.BooksLocation, album.Artist!, album.Title!, "mp3", $"{episode.Title}.mp3");
            if (System.IO.File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                episode.FileLength = fileInfo.Length;
                await _entityRepository.UpdateAsync(episode);
            }
        }
    }
}
