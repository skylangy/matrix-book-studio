using System.Diagnostics;

namespace MatrixBook.Server.Common;

public class ExecutionTimeLoggingMiddleware(RequestDelegate next, ILogger<ExecutionTimeLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExecutionTimeLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();
        var elapsedTime = stopwatch.ElapsedMilliseconds;
        var path = context.Request.Path;
        _logger.LogInformation("Request to '{path}' took {elapsedTime} ms.", path, elapsedTime);
    }
}
