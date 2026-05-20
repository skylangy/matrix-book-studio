using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace MatrixBook.Server.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class RegexController(
    IEntityRepository entityRepository
    ) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;

    [HttpGet("all", Name = "allRegex")]
    public async Task<IEnumerable<RegexModel>> Get()
    {
        return await _entityRepository.GetAllAsync<RegexModel>();
    }

    [HttpGet("{id}", Name = "getRegex")]
    public async Task<RegexModel?> Get(string id)
    {
        return await _entityRepository.GetAsync<RegexModel>(id);
    }

    [HttpPost("add", Name = "addRegex")]
    public async Task<RegexModel> Add([FromBody] RegexModel regex)
    {
        if (string.IsNullOrEmpty(regex.Id))
            regex.Id = Guid.NewGuid().ToString();
        await _entityRepository.UpdateAsync<RegexModel>(regex);

        return regex;
    }

    [HttpDelete("{id}", Name = "removeRegex")]
    public async Task<bool> Remove(string id)
    {
        return await _entityRepository.DeleteAsync<RegexModel>(id);
    }

    [HttpPut("update", Name = "updateRegex")]
    public async Task<RegexModel> Update([FromBody] RegexModel regex)
    {
        if (string.IsNullOrEmpty(regex.Id))
            regex.Id = Guid.NewGuid().ToString();
        await _entityRepository.UpdateAsync<RegexModel>(regex);

        return regex;
    }
}
