using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Models.Data;
using MatrixBook.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController(IEntityRepository entityRepository) : ControllerBase
{
    [HttpGet("", Name = "getDashboard")]
    public async Task<DashboardModel> Get()
    {
        var books = await entityRepository.GetAllAsync<Book>();

        var model = new DashboardModel()
        {
            BookCount = books.Count(),
            FinishedBookCount = books.Count(x => x.Status == BookStatus.Finished),
            InProgressBookCount = books.Count(x => x.Status == BookStatus.InProgress),
            WordCount = books.Sum(x => x.TextCount),
            FinishedWordCount = books.Where(x => x.Status == BookStatus.Finished).Sum(x => x.TextCount),
            UnfinishedWordCount = books.Where(x => x.Status == BookStatus.InProgress).Sum(x => x.TextCount),
            AuthorCount = books.Where(x => !string.IsNullOrWhiteSpace(x.Author)).SelectMany(x => x.Author!).Distinct().Count(),
            Books = books.Select(x => new BookInfo
            {
                Title = x.Title,
                Subtitle = x.Subtitle,
                Author = x.Author,
                Status = x.Status,
                TextCount = x.TextCount,
                DateCreated = x.DateCreated,
                DateUpdated = x.DateUpdated,
            }).ToList(),
        };

        return model;
    }
}
