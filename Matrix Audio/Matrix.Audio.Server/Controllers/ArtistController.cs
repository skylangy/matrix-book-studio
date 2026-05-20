using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ArtistController(
    IEntityRepository entityRepository,
    IMetadataProcessor metadataProcessor,
    ILogger<ArtistController> logger
    ) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IMetadataProcessor _metadataProcessor = metadataProcessor;
    private readonly ILogger<ArtistController> _logger = logger;

    [HttpGet("recents/{page}/{pageSize}", Name = "recentArtists")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetRecents(int page, int pageSize)
    {
        var artists = await _entityRepository.QueryAsync(session => session.Query<Artist>()
                                                                              .OrderByDescending(x => x.DateCreated)
                                                                              .Skip((page - 1) * pageSize)
                                                                              .Take(pageSize));

        var viewModels = artists.ToViewModels();
        return Ok(viewModels);
    }

    [HttpGet("popular/{page}/{pageSize}", Name = "popularArtists")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPopularArtists(int page, int pageSize)
    {
        var artistIds = await _entityRepository.QueryAsync(session => session.Query<Album>()
                                                                                    .OrderByDescending(x => x.PlayCount)
                                                                                    .ThenByDescending(x => x.DateUpdated)
                                                                                    .Select(x => x.ArtistId)
                                                                                    .Distinct()
                                                                                    .Skip((page - 1) * pageSize)
                                                                                    .Take(pageSize)
        );

        var artists = await _entityRepository.QueryAsync(session => session.Query<Artist>().Where(x => x.Id.In(artistIds)));

        var viewModels = artists.ToViewModels();

        return Ok(viewModels);
    }

    [HttpGet("all/{page}/{pageSize}", Name = "allArtists")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAllArtists(int page, int pageSize)
    {
        var artists = await _entityRepository.QueryAsync(session => session.Query<Artist>()
                                                                              .OrderByDescending(x => x.DateUpdated)
                                                                              .Skip((page - 1) * pageSize)
                                                                              .Take(pageSize)
                                                                              );
        var viewModels = artists.ToViewModels();
        return Ok(viewModels);
    }

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchArtist")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 10)
    {
        var artists = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Artist>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Name, keyword);
            }

            return query.OrderByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });

        var viewModels = artists.ToViewModels().ToList();
        return Ok(viewModels);
    }

    [HttpGet("{id}", Name = "getArtist")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(string id)
    {
        var artist = await _entityRepository.GetAsync<Artist>(id);
        var viewModel = artist.ToViewModel();

        viewModel.Albums = await _entityRepository.GetAlbumsForArtist(artist);
        return Ok(viewModel);
    }

    [HttpGet("for/edit/{id}", Name = "getArtistForEdit")]
    public async Task<IActionResult> GetForEdit(string id)
    {
        var artist = await _entityRepository.GetAsync<Artist>(id);
        var viewModel = artist.ToViewModel();
        viewModel.Albums = await _entityRepository.GetAlbumsForArtist(artist);
        return Ok(viewModel);
    }

    [HttpPost("update", Name = "updateArtist")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] Artist artist)
    {
        _logger.LogInformation("Updating artist with id: {}", artist.Id);
        artist.DateUpdated = DateTime.Now;
        var result = await _entityRepository.UpdateAsync(artist);
        if (!result)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpDelete("{id}", Name = "deleteArtist")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting artist with id: {}", id);
        var result = await _entityRepository.DeleteAsync<Artist>(id);
        if (!result)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPost("scan/metadata", Name = "scanArtistMetadata")]
    public async Task<IActionResult> ScanArtists()
    {
        var count = await _metadataProcessor.ScanArtistMetadata();
        return Ok(new ResultBase { Success = true, Message = $"{count} artists processed." });
    }
}
