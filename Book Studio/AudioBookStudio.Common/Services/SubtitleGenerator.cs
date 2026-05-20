using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class SubtitleGenerator(
    IOptions<ScriptConfig> scriptConfig,
    IScriptRunner scriptRunner,
    ILogger<SubtitleGenerator> logger) : ISubtitleGenerator
{
    private readonly ScriptConfig _scriptConfig = scriptConfig.Value;

    public async Task GenerateSubtitlesAsync(SubtitleGenerateContext context)
    {
        logger.LogInformation("Generate subtitle for {}", context.Chapter?.Title);

        await scriptRunner.ExecuteGenerateSubtitle(
            _scriptConfig,
            context.AudioFilePath,
            context.SubtitleFilePath);
    }

    public async Task GenerateSubtitlesFromFolderAsync(SubtitleGenerateContext context)
    {
        logger.LogInformation("Generate subtitle for {} to {}", context.AudioFilePath, context.SrtFolder);
        await scriptRunner.ExecuteGenerateSubtitleAsync(_scriptConfig, context.AudioFilePath, context.SrtFolder);
    }
}
