using System.Diagnostics;

namespace Matrix.Audio.Server.Common;

public class ExecutionTimeLoggingMiddleware(
    RequestDelegate next,
    ILogger<ExecutionTimeLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExecutionTimeLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedTime = stopwatch.ElapsedMilliseconds;
            var path = context.Request.Path;
            _logger.LogInformation("Request took {elapsedTime} ms to '{path}' .", elapsedTime, path);
        }
    }
}
