using Matrix.Audio.Common.Abstraction;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Text;

namespace Matrix.Audio.Common.Extensions;
public static class CacheServiceExtensions
{
    public const string RequestKeyContext = "RequestKey";
    private static readonly string[] sourceArray = ["controller", "action"];

    public static async Task<T> GetAsync<T>(this ICacheService cacheService,
        string key,
        Func<Task<T>>? fallbackValue = null,
        int durationInMinutes = 15)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);

        var result = await cacheService.GetAsync<T>(key);
        if (result != null)
        {
            return result;
        }

        if (fallbackValue == null)
        {
            return default!;
        }

        T value = await fallbackValue();
        await cacheService.SetAsync(key, value, durationInMinutes);

        return value;
    }

    public static string GenerateKey(this ICacheKeyService _, HttpContext httpContext)
    {
        var sb = new StringBuilder();
        var controllerName = httpContext.Request.RouteValues["controller"]?.ToString();
        var actionName = httpContext.Request.RouteValues["action"]?.ToString();

        sb.Append($"{controllerName}:{actionName}");

        var routeData = httpContext.Request.HttpContext.GetRouteData();
        var parameters = routeData?.Values
                .Where(x => !sourceArray.Contains(x.Key))
                .ToDictionary(
                    x => x.Key,
                    x => x.Value?.ToString()
                );
        if (parameters != null && parameters.Count > 0)
        {
            sb.Append(':');
            foreach (var param in parameters)
            {
                sb.Append(param.Key + ":");

                if (!string.IsNullOrEmpty(param.Value))
                    sb.Append(param.Value.ToString() + ":");
            }
        }
        return sb.ToString();
    }

    public static string GetRequestKey(this ICacheKeyService _, HttpContext httpContext)
    {
        var requestKey = httpContext.Items[RequestKeyContext]?.ToString() ?? "";
        return requestKey;
    }

    public static string GetCacheKey(this ICacheKeyService _)
    {
        return RequestKeyContext;
    }
}
