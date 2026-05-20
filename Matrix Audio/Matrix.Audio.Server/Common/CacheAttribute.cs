using Matrix.Audio.Common.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Text;

namespace Matrix.Audio.Server.Common;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheAttribute(int durationMinutes = 1) : Attribute, IAsyncActionFilter
{
    public int DurationMinutes { get; } = durationMinutes;

    public string? KeyPrefix { get; set; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
        var cacheKey = GenerateCacheKey(context);

        var cached = await cacheService.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cached))
        {
            if (cached == CacheSettings.NotFoundKey)
            {
                context.Result = new NotFoundResult();
                return;

            }

            context.Result = new ContentResult
            {
                Content = cached,
                ContentType = "application/json",
                StatusCode = 200
            };
            return;
        }

        var executedContext = await next();

        if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            await cacheService.SetAsync(cacheKey, objectResult.Value, DurationMinutes);
        }
        else if (executedContext.Result is NotFoundResult)
        {
            await cacheService.SetAsync(cacheKey, CacheSettings.NotFoundKey, DurationMinutes);
        }
    }

    private string GenerateCacheKey(ActionExecutingContext context)
    {
        var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var method = descriptor != null
            ? $"{descriptor.ControllerName}.{descriptor.ActionName}"
            : "UnknownMethod";
        var args = context.ActionArguments;

        var keyBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(KeyPrefix))
            keyBuilder.Append(KeyPrefix).Append(':');

        keyBuilder.Append(method);

        foreach (var arg in args)
        {
            keyBuilder.Append('|').Append(arg.Key).Append(':');
            keyBuilder.Append(JsonConvert.SerializeObject(arg.Value));
        }

        var key = keyBuilder.ToString();

        if (key.Length > 200)
            key = $"{KeyPrefix}:{key.GetHashCode()}";

        return key;
    }
}