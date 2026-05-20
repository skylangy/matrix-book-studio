using Matrix.Audio.Common.Abstraction;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace Matrix.Audio.Common.Services;
public class CacheService(
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    IConnectionMultiplexer connectionMultiplexer,
    ILogger<CacheService> logger) : ICacheService
{
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly IDistributedCache _redisCache = distributedCache;
    private readonly IDatabase _redisDatabase = connectionMultiplexer?.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    private readonly ILogger<CacheService> _logger = logger;
    private readonly JsonSerializerSettings _jsonSerializeOption = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };
    private readonly bool _useMemoryCache = false;

    public async Task<string> GetStringAsync(string key)
    {
        var cachedValue = await _redisCache.GetStringAsync(key);
        return cachedValue ?? string.Empty;
    }

    public async Task<T> GetAsync<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (_useMemoryCache && _memoryCache.TryGetValue(key, out T? memoryCacheValue) && memoryCacheValue != null)
        {
            return memoryCacheValue;
        }

        try
        {
            var cachedValue = await _redisCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var deserializedValue = JsonConvert.DeserializeObject<T>(cachedValue);
                if (deserializedValue != null)
                {
                    return deserializedValue;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cache from Redis for key: {key}", key);
        }

        return default!;
    }

    public async Task SetAsync<T>(string key, T value, int durationInMinutes = 15)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var expiration = TimeSpan.FromMinutes(durationInMinutes);

        try
        {
            var serializedValue = JsonConvert.SerializeObject(value, _jsonSerializeOption);
            var redisOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _redisCache.SetStringAsync(key, serializedValue, redisOptions);
            _logger.LogDebug("Successfully set cache in Redis for key '{key}', {expiration}.", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key '{key}' in Redis. Falling back to Memory Cache.", key);
        }

        if (_useMemoryCache)
            _memoryCache.Set(key, value, expiration);
    }

    public async Task SetStringAsync(string key, string value, int durationInMinutes = 15)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var expiration = TimeSpan.FromMinutes(durationInMinutes);

        try
        {
            var redisOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
            await _redisCache.SetStringAsync(key, value, redisOptions);
            _logger.LogDebug("Successfully set cache in Redis for key '{key}', {expiration}.", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key '{key}' in Redis. Falling back to Memory Cache.", key);
        }

        if (_useMemoryCache)
            _memoryCache.Set(key, value, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        await _redisCache.RemoveAsync(key);

        if (_useMemoryCache)
            _memoryCache.Remove(key);
    }

    public async Task RemoveByPatternAsync(string prefix)
    {
        var server = _redisDatabase.Multiplexer.GetServer(_redisDatabase.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{prefix}*");

        await _redisDatabase.KeyDeleteAsync([.. keys]);
    }
}
