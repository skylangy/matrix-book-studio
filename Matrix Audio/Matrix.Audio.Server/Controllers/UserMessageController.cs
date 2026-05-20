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
public class UserMessageController(
    IEntityRepository entityRepository,
    ILogger<UserMessageController> logger)
    : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ILogger<UserMessageController> _logger = logger;

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchUserMessage")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 10)
    {
        var messages = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<UserMessage>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Subject, keyword)
                             .Search(x => x.Content, keyword);
            }

            return query.OrderByDescending(x => x.DateCreated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });
        return Ok(messages);
    }

    [HttpGet("{page}/{pageSize}", Name = "getMessages")]
    public async Task<IActionResult> GetMessagesAsync(int page = 1, int pageSize = 10)
    {
        var messages = await _entityRepository.QueryAsync(session => session.Query<UserMessage>()
                                                                        .OrderByDescending(x => x.DateCreated)
                                                                        .Skip((page - 1) * pageSize)
                                                                        .Take(pageSize));
        return Ok(messages);
    }

    [HttpPost("send", Name = "sendMessage")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] UserMessageViewModel message)
    {
        if (string.IsNullOrWhiteSpace(message.Content) || string.IsNullOrWhiteSpace(message.UserId))
        {
            return BadRequest(new ResultBase { Success = false, Message = "Message content and recipient email are required." });
        }

        _logger.LogInformation("User with ID: {UserId} send a message", message.UserId);
        await _entityRepository.UpdateAsync<UserMessage>(new()
        {
            Id = Guid.NewGuid().ToString(),
            UserId = message.UserId,
            Subject = message.Subject,
            Content = message.Content,
            DateCreated = DateTime.UtcNow
        });

        return Ok(new ResultBase { Success = true, Message = "Message sent successfully." });
    }

    [HttpGet("{id}", Name = "getMessage")]
    public async Task<IActionResult> GetMessage(string id)
    {
        var message = await _entityRepository.GetAsync<UserMessage>(id);
        if (message == null)
        {
            return NotFound(new ResultBase { Success = false, Message = "Message not found." });
        }
        return Ok(message);
    }
}