using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LoggingController(IWorkProgressRepository workProgressRepository) : ControllerBase
{
    private readonly IWorkProgressRepository _workProgressRepository = workProgressRepository;

    [HttpGet("{category}", Name = "getWorkprogressByCategory")]
    public async Task<IEnumerable<WorkProgress>> Get(string category)
    {
        return await _workProgressRepository.GetByCategoryAsync(category);
    }
}
