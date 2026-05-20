using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class FaqController(IEntityRepository entityRepository,
     ILogger<FaqController> logger) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ILogger<FaqController> _logger = logger;

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchFaqs")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 10)
    {
        var faqs = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Faq>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Question, keyword)
                             .Search(x => x.Answer, keyword);
            }

            return query.OrderByDescending(x => x.DateCreated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });
        return Ok(faqs);
    }

    [HttpGet("all/{page}/{pageSize}", Name = "getAllFaqs")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAllFaqs(int page = 1, int pageSize = 12)
    {
        var faqs = await _entityRepository.QueryAsync(session => session.Query<Faq>()
                                                                        .OrderByDescending(x => x.DateCreated)
                                                                        .Skip((page - 1) * pageSize)
                                                                        .Take(pageSize));
        return Ok(faqs);
    }

    [HttpGet("admin/all/{page}/{pageSize}", Name = "getAllFaqsForAdmin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllFaqsForAdmin(int page = 1, int pageSize = 12)
    {
        var faqs = await _entityRepository.QueryAsync(session => session.Query<Faq>()
                                                                        .OrderByDescending(x => x.DateCreated)
                                                                        .Skip((page - 1) * pageSize)
                                                                        .Take(pageSize));
        return Ok(faqs);
    }

    [HttpGet("{id}", Name = "getFaq")]
    public async Task<IActionResult> GetFaq(string id)
    {
        var faq = await _entityRepository.GetAsync<Faq>(id);
        return Ok(faq);
    }

    [HttpPut("update", Name = "updateFaq")]
    [Authorize]
    public async Task<IActionResult> UpdateFaq([FromBody] Faq faq)
    {
        if (faq == null)
        {
            return BadRequest(new ResultBase { Success = false, Message = "FAQ is null" });
        }

        faq.DateUpdated = DateTime.Now;
        await _entityRepository.UpdateAsync(faq);
        return Ok(new ResultBase { Success = true, Message = "FAQ updated" });
    }

    [HttpDelete("{id}", Name = "deleteFaq")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deleting FAQ with id: {}", id);
        await _entityRepository.DeleteAsync<Faq>(id);
        return Ok(new ResultBase { Success = true, Message = "FAQ is deleted" });
    }
}
