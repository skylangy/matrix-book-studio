using Microsoft.AspNetCore.Mvc.Filters;

namespace Matrix.Audio.Server.Common;

public class ImageHeaderFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Do nothing before the action executes
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var response = context.HttpContext.Response;
        response.Headers.Append("Cache-Control", "public,max-age=3600");
        response.Headers.Append("ETag", "your-etag-value");
        response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));
    }
}
