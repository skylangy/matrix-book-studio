using MatrixBook.Server.Models;
using MatrixBook.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OptionsController(IOptionsService optionsService) : ControllerBase
{
    [HttpGet("all", Name = "allOptions")]
    public async Task<OptionCollection> Get()
    {
        return await optionsService.GetOptionsAsync();
    }

    [HttpPut("update", Name = "updateOptions")]
    public async Task<OptionCollection> Update([FromBody] OptionCollection options)
    {
        return await optionsService.UpdateOptionsAsync(options);
    }
}
