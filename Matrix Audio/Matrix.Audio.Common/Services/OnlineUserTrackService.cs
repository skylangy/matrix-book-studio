using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Matrix.Audio.Common.Services;
public class OnlineUserTrackService(
    IOptions<RedisConfig> redisConfig,
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer) : IOnlineUserTrackService
{
    private const string UserActivityKey = "UserActivity";
    private const int OnlineTimeoutMinutes = 30;

    private readonly RedisConfig _redisConfig = redisConfig?.Value ?? throw new ArgumentNullException(nameof(redisConfig));
    private readonly IDistributedCache _redisCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
    private readonly IDatabase _redisDatabase = connectionMultiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly DistributedCacheEntryOptions _cacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(OnlineTimeoutMinutes)
    };

    private string GetRedisKey(string userId) => $"{UserActivityKey}:{userId}";

    public async Task<int> GetOnlineUserCountAsync()
    {
        try
        {
            var onlineUsers = await GetOnlineUsersAsync();
            return onlineUsers.Count();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error fetching online user count.", ex);
        }
    }

    public async Task<IEnumerable<string>> GetOnlineUsersAsync()
    {
        try
        {
            var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{_redisConfig.InstanceName}{UserActivityKey}:*")
                             .Select(k => k.ToString().Split(':').Last())
                             .ToList();
            return await Task.FromResult(keys);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error fetching online users.", ex);
        }
    }

    public async Task<bool> IsUserOnlineAsync(string userId)
    {
        try
        {
            var redisKey = GetRedisKey(userId);
            var timestamp = await _redisCache.GetStringAsync(redisKey);
            return timestamp != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking online status for user {userId}.", ex);
        }
    }

    public async Task UpdateUserActivityAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            var redisKey = GetRedisKey(userId);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            await _redisCache.SetStringAsync(redisKey, timestamp, _cacheEntryOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating activity for user {userId}.", ex);
        }
    }
}

