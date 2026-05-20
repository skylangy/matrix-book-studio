using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services;
public class ContentWriter(ILogger<ContentWriter> logger) : IContentWriter
{
    private readonly ILogger<ContentWriter> _logger = logger;

    public Task<bool> WriteAsync(WriteContentContext context)
    {
        try
        {
            using (var writer = new StreamWriter(context.Path))
                writer.Write(context.Content);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing content to file");
            return Task.FromResult(false);
        }
    }
}
