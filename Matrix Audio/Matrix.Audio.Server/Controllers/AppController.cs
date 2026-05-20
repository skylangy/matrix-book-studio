using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AppController(
    IConfiguration configuration,
    IEntityRepository entityRepository,
    IOnlineUserTrackService onlineUserTrackService) : ControllerBase
{
    private const string CachePrefix = "App";
    private readonly IConfiguration _configuration = configuration;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IOnlineUserTrackService _onlineUserTrackService = onlineUserTrackService;

    [HttpGet("summary", Name = "getAppSummary")]
    [Cache(KeyPrefix = CachePrefix)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetSummary()
    {
        var albumCount = await _entityRepository.CountAsync(session => session.Query<Album>());
        var episodeCount = await _entityRepository.CountAsync(session => session.Query<Episode>());
        var artistCount = await _entityRepository.CountAsync(session => session.Query<Artist>());
        var postCount = await _entityRepository.CountAsync(session => session.Query<Post>());
        var promoCount = await _entityRepository.CountAsync(session => session.Query<Promo>());
        var messageCount = await _entityRepository.CountAsync(session => session.Query<UserMessage>());
        var userCount = await _entityRepository.CountAsync(session => session.Query<User>());
        var fqaCount = await _entityRepository.CountAsync(session => session.Query<Faq>());
        var orderCount = await _entityRepository.CountAsync(session => session.Query<Order>());
        var subCount = await _entityRepository.CountAsync(session => session.Query<SubscriptionPlan>());
        var duration = await _entityRepository.GetTotalDurationInHour();
        var onlineUsers = await _onlineUserTrackService.GetOnlineUserCountAsync();

        var model = new SummaryViewModel
        {
            AlbumCount = albumCount,
            EpisodeCount = episodeCount,
            DurationInHour = duration,
            AuthorCount = artistCount,
            PostCount = postCount,
            PromoCount = promoCount,
            UserMessageCount = messageCount,
            UserCount = userCount,
            FaqCount = fqaCount,
            OrderCount = orderCount,
            SubscriptionCount = subCount,
            OnlineUserCount = onlineUsers
        };

        return Ok(model);
    }

    [HttpGet("settings", Name = "getAppSettings")]
    [Cache(KeyPrefix = CachePrefix)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetAppSettings()
    {
        var settings = await _entityRepository.GetAppSettingsFromConfigAsync(_configuration);
        return Ok(settings);
    }

    [HttpGet("settings/{id}", Name = "getAppSetting")]
    [Cache(KeyPrefix = CachePrefix)]
    public async Task<IActionResult> GetAppSetting(string id)
    {
        var settings = await _entityRepository.GetAsync<AppSetting>(id);
        return Ok(settings);
    }

    [HttpPost("populate/settings/from/config", Name = "populateSettingsFromConfig")]
    //[Authorize]
    public async Task<IActionResult> UpdateSettings()
    {
        await _entityRepository.PopulateAppSettingsFromConfigAsync(_configuration);
        var result = new ResultBase { Success = true, Message = "App settings are initialized" };
        return Ok(result);
    }
}
