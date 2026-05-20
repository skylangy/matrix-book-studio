using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services.Fake;

public class FakeTextToVoiceService(ILogger<FakeTextToVoiceService> logger) : ITextToVoiceService
{
    public async Task<TextToVoiceResult> Convert(string content, TextToVoiceServiceContext context)
    {
        var result = new TextToVoiceResult
        {
            OutputFile = Path.Combine(context.OutputPath, context.FileName),
            Success = true
        };

        await result.OutputFile.SafeWrite(content, (ex) =>
        {
            logger.LogError(ex, "Convert text to WAV failed.");
        });

        logger.LogInformation("Finish converting text to WAV at {}, with {} and {}", result.OutputFile, context.Language, context.VoiceName);

        return result;
    }
}