using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services.Fake;

public class FakeVideoGenerator(ILogger<FakeVideoGenerator> logger) : IVideoGenerator
{
    public async Task<VideoGeneratorResult> Generate(VideoGeneratorContext context)
    {
        var result = new VideoGeneratorResult
        {
            OutputFile = context.VideoFile
        };

        using var outStream = new FileStream(context.VideoFile, FileMode.Create, FileAccess.Write);
        using var reader = new FileStream(context.AudioFile, FileMode.Open, FileAccess.Read);
        await reader.CopyToAsync(outStream);
        outStream.Flush();
        result.Success = true;

        logger.LogInformation("Generate video at [{}] from [{}].", result.OutputFile, context.AudioFile);

        return result;
    }
}