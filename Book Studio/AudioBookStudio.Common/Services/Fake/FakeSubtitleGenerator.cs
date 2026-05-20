using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services.Fake;
public class FakeSubtitleGenerator(ILogger<FakeVideoGenerator> logger) : ISubtitleGenerator
{
    public Task GenerateSubtitlesAsync(SubtitleGenerateContext context)
    {
        logger.LogInformation("Generate subtitle for {}", context.Chapter?.Title);
        return Task.CompletedTask;
    }

    public Task GenerateSubtitlesFromFolderAsync(SubtitleGenerateContext context)
    {
        return Task.CompletedTask;
    }
}
