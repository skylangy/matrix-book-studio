using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TemplateController(IEntityRepository entityRepository,
    ILogger<TemplateController> logger) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ILogger<TemplateController> _logger = logger;

    [HttpGet("{page?}/{pageSize?}", Name = "getAllVideoTemplates")]
    public async Task<IEnumerable<VideoTemplate>> GetAll(int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var videos = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<VideoTemplate>().OrderByDescending(x => x.DateUpdated);

            return query;
        });

        var pagedVideos = videos
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return pagedVideos;
    }

    [HttpGet("search", Name = "searchVideoTemplates")]
    public async Task<IEnumerable<VideoTemplate>> Search([FromQuery] string keyword = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var videos = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<VideoTemplate>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Name, keyword).Search(x => x.Description, keyword);

            }
            return query;
        });

        var pagedVideos = videos
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        return pagedVideos;
    }

    [HttpGet("{id}", Name = "getVideoTemplateById")]
    public async Task<IActionResult> Get(string id)
    {
        var video = await _entityRepository.GetByIdAsync<VideoTemplate>(id);
        if (video == null)
            return NotFound();
        return Ok(video);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VideoTemplate template)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var now = DateTime.Now;
        if (!template.DateCreated.HasValue)
            template.DateCreated = now;
        template.DateUpdated = now;
        await _entityRepository.UpdateAsync(template);
        return Ok(template);
    }

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] VideoTemplate template)
    {
        var existing = await _entityRepository.GetByIdAsync<VideoTemplate>(template.Id);
        var now = DateTime.Now;
        if (existing == null)
        {
            if (!template.DateCreated.HasValue)
                template.DateCreated = now;
            template.DateUpdated = now;
            await _entityRepository.UpdateAsync(template);
            return Ok(template);
        }

        if (!template.DateCreated.HasValue)
            template.DateCreated = now;
        template.DateUpdated = now;

        await _entityRepository.UpdateAsync(template);
        return Ok(template);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var video = await _entityRepository.GetByIdAsync<VideoTemplate>(id);
        if (video == null)
            return Ok(false);

        await _entityRepository.DeleteAsync<VideoTemplate>(id);
        return Ok(true);
    }
}
