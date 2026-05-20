using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using System.Text.Json;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController(
    IOptions<AppConfiguration> appConfiguration,
    IEntityRepository entityRepository,
    IOnlineUserTrackService onlineUserTrackService,
    ILogger<UserController> logger
    ) : ControllerBase
{
    private readonly AppConfiguration _appConfiguration = appConfiguration.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IOnlineUserTrackService _onlineUserTrackService = onlineUserTrackService;
    private readonly ILogger<UserController> _logger = logger;

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchUsers")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 10)
    {
        var users = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<User>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Name, keyword);
            }

            return query.OrderByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });
        UserProfile profileRetriever(string id) => _entityRepository.GetProfileForUserAsync(id).Result;
        var viewModels = users.ToViewModels(profileRetriever).ToList();
        return Ok(viewModels);
    }

    [HttpGet("{page}/{pageSize}", Name = "getAllUsers")]
    public async Task<IActionResult> Get(int page = 1, int pageSize = 10)
    {
        var users = await _entityRepository.QueryAsync(session => session.Query<User>()
                                                                          .OrderByDescending(x => x.DateCreated)
                                                                          .Skip((page - 1) * pageSize)
                                                                          .Take(pageSize));
        return Ok(users);
    }

    [HttpGet("online/{page}/{pageSize}", Name = "getOnlineUsers")]
    public async Task<IActionResult> GetOnlineUsers(int page = 1, int pageSize = 10)
    {
        var userNames = await _onlineUserTrackService.GetOnlineUsersAsync();

        var users = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<User>();

            foreach (var userEmail in userNames)
            {
                query = query.Search(x => x.Email, userEmail);
            }
            return query.OrderByDescending(x => x.DateCreated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });
        return Ok(users);
    }

    [HttpPost("favorite/album/{userId}/{albumId}", Name = "favoriteAlbum")]
    public async Task<IActionResult> FavoriteAlbum(string userId, string albumId)
    {
        var userInfo = CreateUserOperation(userId, albumId, UserOperationType.Favorite);

        var result = await _entityRepository.UpdateAsync(userInfo);

        var album = await _entityRepository.GetAsync<Album>(albumId);
        album.Ranking += 1;
        await _entityRepository.UpdateAsync(album);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Favorite album success" : "Favorite album failed"
        });
    }

    [HttpPost("unfavorite/album/{userId}/{albumId}", Name = "unfavoriteAlbum")]
    public async Task<IActionResult> UnFavoriteAlbum(string userId, string albumId)
    {
        var result = await _entityRepository.DeleteUserOperationAsync(userId, albumId, UserOperationType.Favorite);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Unfavorite album success" : "Unfavorite album failed"
        });
    }

    [HttpPost("like/album/{userId}/{albumId}", Name = "likeAlbum")]
    public async Task<IActionResult> LikeAlbum(string userId, string albumId)
    {
        var userInfo = CreateUserOperation(userId, albumId, UserOperationType.Like);

        var result = await _entityRepository.UpdateAsync(userInfo);

        var album = await _entityRepository.GetAsync<Album>(albumId);
        album.LikeCount += 1;
        await _entityRepository.UpdateAsync(album);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Like album success" : "Like album failed"
        });
    }

    [HttpPost("playlist/add/{userId}/{albumId}", Name = "addToPlaylist")]
    public async Task<IActionResult> AddToPlaylist(string userId, string albumId)
    {
        var userInfo = CreateUserOperation(userId, albumId, UserOperationType.PlayList);

        var result = await _entityRepository.UpdateAsync(userInfo);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Add to playlist success" : "Add to playlist failed"
        });
    }

    [HttpPost("playlist/remove/{userId}/{albumId}", Name = "removeFromPlaylist")]
    public async Task<IActionResult> RemoveFromPlaylist(string userId, string albumId)
    {
        var result = await _entityRepository.DeleteUserOperationAsync(userId, albumId, UserOperationType.PlayList);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Remove from playlist success" : "Remove from playlist failed"
        });
    }

    [HttpPost("download/episode/{userId}/{albumId}/{episodeId}", Name = "downloadEpisode")]
    public async Task<IActionResult> DownloadEpisode(string userId, string albumId, string episodeId)
    {
        var userInfo = CreateUserOperation(userId, episodeId, UserOperationType.DownloadEpisode);

        var updateResult = await _entityRepository.UpdateAsync(userInfo);
        _logger.LogInformation("Download episode '{episodeId}' for user '{userId}' with result '{result}'", episodeId, userId, updateResult);

        var album = await _entityRepository.GetAsync<Album>(albumId);
        var episode = await _entityRepository.GetAsync<Episode>(episodeId);
        if (episode == null)
        {
            return new NotFoundResult();
        }

        var result = episode.GetEpisodeStreamAsync(_appConfiguration.BooksLocation, album.Artist!, album.Title!, _logger);
        return result;
    }

    [HttpPost("download/album/{userId}/{albumId}", Name = "downloadAlbum")]
    public async Task<IActionResult> DownloadAlbum(string userId, string albumId)
    {
        var userInfo = CreateUserOperation(userId, albumId, UserOperationType.DownloadAlbum);

        var result = await _entityRepository.UpdateAsync(userInfo);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Download album success" : "Download album failed"
        });
    }

    [HttpGet("subscription/{userId}", Name = "getUserSubscription")]
    [Cache(CacheSettings.CacheDurationInMinuteShort)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetUserSubscription(string userId)
    {
        var subscription = await _entityRepository.GetUserSubscriptionAsync(userId);
        return Ok(subscription);
    }

    [HttpGet("history/{userId}/{page}/{count}", Name = "getHistory")]
    [Cache(CacheSettings.CacheDurationInMinuteShort)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetHistory(string userId, int page = 1, int count = 12)
    {
        var albumIds = await _entityRepository.QueryAsync(session => session.Query<UserOperation>()
                                                                            .Where(x => x.Operation == UserOperationType.PlayAlbum && x.UserId == userId)
                                                                            .OrderByDescending(x => x.DateCreated)
                                                                            .Select(x => x.TargetId)
                                                                            .Distinct()
                                                                            .Skip((page - 1) * count)
                                                                            .Take(count));

        var viewModels = await _entityRepository.QueryAlbumsByIds(albumIds);

        return Ok(viewModels);
    }

    [HttpGet("favorites/{userId}/{page}/{count}", Name = "getFavorites")]
    [Cache(CacheSettings.CacheDurationInMinuteShort)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetFavorites(string userId, int page = 1, int count = 12)
    {
        var albumIds = await _entityRepository.QueryAsync(session => session.Query<UserOperation>()
                                                                               .Where(x => x.Operation == UserOperationType.Favorite && x.UserId == userId)
                                                                               .OrderByDescending(x => x.DateCreated)
                                                                               .Select(x => x.TargetId)
                                                                               .Distinct()
                                                                               .Skip((page - 1) * count)
                                                                               .Take(count));

        var viewModels = await _entityRepository.QueryAlbumsByIds(albumIds);

        var result = albumIds.Select(x => viewModels.First(m => m.Id == x));
        return Ok(result);
    }

    [HttpGet("playlist/{userId}/{page}/{count}", Name = "getPlaylist")]
    [Cache(CacheSettings.CacheDurationInMinuteShort)]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPlayList(string userId, int page = 1, int count = 12)
    {
        var albumIds = await _entityRepository.QueryAsync(session => session.Query<UserOperation>()
                                                                               .Where(x => x.Operation == UserOperationType.PlayList && x.UserId == userId)
                                                                               .OrderByDescending(x => x.DateCreated)
                                                                               .Select(x => x.TargetId)
                                                                               .Distinct()
                                                                               .Skip((page - 1) * count)
                                                                               .Take(count));
        var viewModels = await _entityRepository.QueryAlbumsByIds(albumIds);

        var result = albumIds.Select(x => viewModels.First(m => m.Id == x));
        return Ok(result);
    }

    [HttpGet("record/download/album/{userId}/{page}/{count}", Name = "getDownloadAlbums")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetDownloadAlbums(string userId, int page = 1, int count = 12)
    {
        var albumIds = await _entityRepository.QueryAsync(session => session.Query<UserOperation>()
                                                                               .Where(x => x.Operation == UserOperationType.DownloadAlbum && x.UserId == userId)
                                                                               .OrderByDescending(x => x.DateCreated)
                                                                               .Select(x => x.TargetId)
                                                                               .Distinct()
                                                                               .Skip((page - 1) * count)
                                                                               .Take(count));

        var viewModels = await _entityRepository.QueryAlbumsByIds(albumIds);
        var result = albumIds.Select(x => viewModels.First(m => m.Id == x));
        return Ok(result);
    }

    [HttpGet("record/download/episode/{userId}/{page}/{count}", Name = "getDownloadEpisodes")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetDownloadEpisodes(string userId, int page = 1, int count = 12)
    {
        var episodeIds = await _entityRepository.QueryAsync(session => session.Query<UserOperation>()
                                                                               .Where(x => x.Operation == UserOperationType.DownloadEpisode && x.UserId == userId)
                                                                               .OrderByDescending(x => x.DateCreated)
                                                                               .Select(x => x.TargetId)
                                                                               .Distinct()
                                                                               .Skip((page - 1) * count)
                                                                               .Take(count));

        var episodes = await _entityRepository.QueryAsync(session => session.Query<Episode>().Where(x => x.Id.In(episodeIds)));


        var viewModels = new List<EpisodeViewModel>();

        var result = episodeIds.Select(x => viewModels.First(m => m.Id == x));
        return Ok(result);
    }

    [HttpPost("record/play", Name = "recordPlay")]
    public async Task<IActionResult> RecordPlay([FromBody] RecordPlayViewModel model)
    {
        var episodeOperation = await _entityRepository.QueryOneAsync(session => session.Query<UserOperation>().Where(x => x.UserId == model.UserId && x.TargetId == model.EpisodeId));

        if (episodeOperation == null)
        {
            episodeOperation = CreateUserOperation(model.UserId, model.EpisodeId, UserOperationType.PlayEpisode, model.AlbumId);
        }
        else
        {
            episodeOperation.Count += 1;
            episodeOperation.DateCreated = DateTime.Now;
        }

        episodeOperation.Details = JsonSerializer.Serialize(model);
        var result = await _entityRepository.UpdateAsync(episodeOperation);


        var albumOperation = await _entityRepository.QueryOneAsync(session => session.Query<UserOperation>().Where(x => x.UserId == model.UserId && x.TargetId == model.AlbumId));

        var album = await _entityRepository.GetAsync<Album>(model.AlbumId);
        album.PlayCount += 1;
        await _entityRepository.UpdateAsync(album);

        var episode = await _entityRepository.GetAsync<Episode>(model.EpisodeId);
        if (episode != null)
        {
            episode.PlayCount += 1;
            await _entityRepository.UpdateAsync(episode);
        }

        if (albumOperation == null)
        {
            albumOperation = CreateUserOperation(model.UserId, model.AlbumId, UserOperationType.PlayAlbum);
        }
        else
        {
            albumOperation.Count += 1;
            albumOperation.DateCreated = DateTime.Now;
        }
        result = await _entityRepository.UpdateAsync(albumOperation);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Record play success" : "Record play failed"
        });
    }

    [HttpGet("record/play/{userId}/{episodId}", Name = "getPlayRecord")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPlayRecord(string userId, string episodId)
    {
        var operationInfo = await _entityRepository.QueryOneAsync(session => session.Query<UserOperation>()
                                                                                       .Where(x => x.UserId == userId && x.TargetId == episodId));

        if (operationInfo == null)
        {
            return NotFound();
        }

        var model = JsonSerializer.Deserialize<RecordPlayViewModel>(operationInfo.Details);

        return Ok(model);
    }

    [HttpGet("record/play/album/{userId}/{albumId}", Name = "getPlayRecordForAlbum")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPlayRecordForAlbum(string userId, string albumId)
    {
        var operationInfo = await _entityRepository.QueryAsync(session =>
        {
            var episodeIds = session.Query<Episode>().Where(x => x.AlbumId == albumId).Select(x => x.Id);

            var query = session.Query<UserOperation>().Where(x => x.UserId == userId && x.TargetId.In(episodeIds));

            return query;
        });

        var models = new List<RecordPlayViewModel>();
        foreach (var operation in operationInfo)
        {
            var model = JsonSerializer.Deserialize<RecordPlayViewModel>(operation.Details);
            if (model != null)
                models.Add(model);
        }
        return Ok(models);
    }

    [HttpPost("subscribe/{email}", Name = "subscribeEmail")]
    public async Task<IActionResult> SubscribeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new ResultBase { Success = false, Message = "Email is required." });
        }

        // Here you would typically check if the email is already subscribed
        var existingSubscription = await _entityRepository.GetSubscriptionByEmailAsync(email);

        if (existingSubscription != null)
        {
            return Ok(new ResultBase { Success = true, Message = "Email is already subscribed." });
        }

        var result = await _entityRepository.UpdateAsync<EmailSubscribe>(new()
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            DateSubscribed = DateTime.UtcNow,
            IsActive = true
        });

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Subscription successful." : "Subscription failed."
        });
    }

    [HttpGet("settings/{userId}", Name = "getUserSettings")]
    [Cache]
    //[ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetUserSettings(string userId)
    {
        var settings = await _entityRepository.GetSettingsForUserAsync(userId);

        if (settings == null)
        {
            settings = new UserSettings
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId
            };
            await _entityRepository.UpdateAsync(settings);
        }
        return Ok(settings);
    }

    [HttpPost("settings", Name = "updateUserSettings")]
    [Authorize]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettings settings)
    {
        var result = await _entityRepository.UpdateAsync(settings);
        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Settings updated." : "Settings update failed."
        });
    }

    [HttpPost("avatar/{userId}/{imageId}", Name = "updateAvatar")]
    [Authorize]
    public async Task<IActionResult> UpdateAvatar(string userId, string imageId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(imageId))
        {
            return BadRequest(new ResultBase { Success = false, Message = "User or image not found." });
        }
        var user = await _entityRepository.GetAsync<User>(userId);
        if (user == null)
        {
            return NotFound(new ResultBase { Success = false, Message = "User not found." });
        }

        user.ImageId = imageId;
        var result = await _entityRepository.UpdateAsync(user);

        return Ok(new ResultBase
        {
            Success = result,
            Message = result ? "Avatar updated." : "Avatar update failed."
        });
    }

    [HttpPost("assign/subscription", Name = "assignSubscription")]
    public async Task<IActionResult> AssignSubscription(UserSubscriptionViewModel viewModel)
    {
        var subscription = await _entityRepository.GetAsync<SubscriptionPlan>(viewModel.SubscriptionId);
        if (subscription == null)
        {
            return NotFound(new ResultBase { Success = false, Message = "Subscription not found" });
        }

        var user = await _entityRepository.GetAsync<User>(viewModel.UserId);
        if (user == null)
        {
            return NotFound(new ResultBase { Success = false, Message = "User not found" });
        }

        var userSubscription = await _entityRepository.GetUserSubscriptionAsync(viewModel.UserId);
        var now = DateTime.UtcNow;
        if (userSubscription == null)
        {
            userSubscription = new UserSubscription
            {
                Id = Guid.NewGuid().ToString(),
                UserId = viewModel.UserId,
                SubscriptionId = viewModel.SubscriptionId,
                Name = subscription.Name,
                IsActive = true,
                PeriodInDays = viewModel.PeriodInDays,
                DateCreated = now,
                StartDate = now,
                EndDate = now.AddDays(viewModel.PeriodInDays)
            };
            await _entityRepository.UpdateAsync(userSubscription);
        }
        else
        {
            userSubscription.SubscriptionId = viewModel.SubscriptionId;
            userSubscription.StartDate = now;
            userSubscription.EndDate = now.AddDays(viewModel.PeriodInDays);
            userSubscription.PeriodInDays = viewModel.PeriodInDays;
            await _entityRepository.UpdateAsync(userSubscription);
        }

        user.Level = subscription.Level;
        await _entityRepository.UpdateAsync(user);

        return Ok(new ResultBase { Success = true, Message = "Subscription assigned." });
    }

    private static UserOperation CreateUserOperation(string userId,
        string targetId,
        string operation,
        string secondaryId = "",
        string details = "")
    {
        return new UserOperation
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            TargetId = targetId,
            SecondaryId = secondaryId,
            Operation = operation,
            Details = details
        };
    }

    private async Task<int> GetAlbumEpisodeCount(string albumId)
    {
        var count = await _entityRepository.CountAsync(session => session.Query<Episode>()
                                                                            .Where(x => x.AlbumId == albumId));
        return count;
    }
}
