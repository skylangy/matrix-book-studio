using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Extensions;

namespace Matrix.Audio.Server.Common;

public class RequestKeyMiddleware(RequestDelegate next, ILogger<RequestKeyMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestKeyMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, ICacheKeyService cacheKeyService)
    {
        var requestKey = cacheKeyService.GenerateKey(context);
        var cacheKey = cacheKeyService.GetCacheKey();

        context.Items[cacheKey] = requestKey;

        // Optional: add it to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd("X-Request-Key", requestKey);
            return Task.CompletedTask;
        });

        // Log within scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [cacheKey] = requestKey
        }))
        {
            _logger.LogInformation("Request key generated: {RequestKey}", requestKey);
            await _next(context);
        }
    }
}

