using Matrix.Audio.Common.Abstraction;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Matrix.Audio.Server.Common;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ResetCacheAttribute : Attribute, IAsyncActionFilter
{
    public string KeyPrefix { get; }

    public ResetCacheAttribute(string keyPrefix)
    {
        KeyPrefix = keyPrefix;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if (executedContext.Exception == null)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

            await cacheService.RemoveByPatternAsync(KeyPrefix);
        }
    }
}