using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AlbumCollectionController(
    IEntityRepository entityRepository) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;

    [HttpGet("{page}/{pageSize}", Name = "getPagedAlbumCollections")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAlbumCollections(int page, int pageSize)
    {
        var viewModels = await _entityRepository.GetPagedAlbumCollections(page, pageSize);

        return Ok(viewModels);
    }

    [HttpGet("admin/{page}/{pageSize}", Name = "getPagedAlbumCollectionsForAdmin")]
    public async Task<IActionResult> GetAlbumCollectionsForAdmin(int page, int pageSize)
    {
        var viewModels = await _entityRepository.GetPagedAlbumCollections(page, pageSize);

        return Ok(viewModels);
    }

    [HttpGet("{id}", Name = "getAlbumCollection")]
    [Cache(CacheSettings.CacheDurationInMinuteLong)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAlbumCollection(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Invalid album collection ID.");
        }

        var viewModel = await _entityRepository.GetAlbumCollection(id);

        if (viewModel == null)
        {
            return NotFound();
        }

        return Ok(viewModel);
    }

    [HttpGet("admin/{id}", Name = "getAlbumCollectionAdmin")]
    public async Task<IActionResult> GetAlbumCollectionAdmin(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Invalid album collection ID.");
        }

        var viewModel = await _entityRepository.GetAlbumCollection(id);

        if (viewModel == null)
        {
            return NotFound();
        }

        return Ok(viewModel);
    }

    [HttpPost(Name = "updateAlbumCollection")]
    public async Task<IActionResult> UpdateAlbumCollection([FromBody] AlbumCollection albumCollection)
    {
        if (albumCollection == null)
        {
            return BadRequest("Album collection view model is null");
        }

        if (string.IsNullOrEmpty(albumCollection.Id))
        {
            albumCollection.Id = Guid.NewGuid().ToString();
            albumCollection.DateCreated = DateTime.Now;
        }

        albumCollection.DateUpdated = DateTime.Now;
        await _entityRepository.UpdateAsync(albumCollection);

        return Ok(ResultBase.Ok());
    }

    [HttpDelete("{id}", Name = "deleteAlbumCollection")]
    public async Task<IActionResult> DeleteAlbumCollection(string id)
    {
        var albumCollection = await _entityRepository.GetAsync<AlbumCollection>(id);
        if (albumCollection == null)
        {
            return NotFound($"Now Album collection found with id: {id}");
        }
        await _entityRepository.DeleteAsync<AlbumCollection>(id);
        return Ok(ResultBase.Ok());
    }
}
