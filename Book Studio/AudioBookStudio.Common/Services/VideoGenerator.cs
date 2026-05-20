using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace AudioBookStudio.Common.Services;

public class VideoGenerator(
    IOptions<AppConfiguration> appConfig,
    ICommandInvoker commandInvoker,
    ILogger<VideoGenerator> logger) : IVideoGenerator
{
    private const string FFMpeg = "ffmpeg";
    private readonly AppConfiguration _appConfig = appConfig.Value;
    private readonly ICommandInvoker _commandInvoker = commandInvoker;
    private readonly ILogger<VideoGenerator> _logger = logger;

    public async Task<VideoGeneratorResult> Generate(VideoGeneratorContext context)
    {
        var result = new VideoGeneratorResult();

        try
        {
            var start = DateTime.Now;

            var cmdParameters = _appConfig.UseGpu ? BuildGpuCommandParameters(context) : BuildCpuCommandParameters(context);
            var command = new CommandModel { Arguments = cmdParameters, Command = FFMpeg };

            _logger.LogInformation("FFMpeg command: ffmpeg {Command}", command.Arguments);
            await _commandInvoker.InvokeAsync(command);
            result.Duration = DateTime.Now - start;
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generate video failed for {}", context.AudioFile);
            result.Error = ex.Message;
            result.Success = false;
        }

        return result;
    }

    private static string BuildCpuCommandParameters(VideoGeneratorContext context)
    {
        var commandBuilder = new StringBuilder();
        commandBuilder.Append(@$"-r 1 ");
        commandBuilder.Append(@$"-loop 1 ");
        commandBuilder.Append(@$"-y ");
        commandBuilder.Append(@$"-i ""{GetImage(context)}"" ");
        commandBuilder.Append(@$"-i ""{context.AudioFile}"" ");
        commandBuilder.Append($@"-metadata title=""{context.MetadataTitle}"" ");
        commandBuilder.Append("-tune stillimage ");
        commandBuilder.Append("-vf scale=1280x720 ");
        commandBuilder.Append(@$"-c:a copy ");
        commandBuilder.Append("-c:v libx264 -crf 18 -pix_fmt yuv420p ");
        commandBuilder.Append("-shortest ");
        commandBuilder.Append(@$"""{context.VideoFile}"" ");
        return commandBuilder.ToString();
    }

    private static string BuildGpuCommandParameters(VideoGeneratorContext context)
    {
        var commandBuilder = new StringBuilder();
        commandBuilder.Append("-y ");
        commandBuilder.Append($"-i \"{GetImage(context)}\" ");
        commandBuilder.Append($"-i \"{context.AudioFile}\" ");
        commandBuilder.Append($"-metadata title=\"{context.MetadataTitle}\" ");
        commandBuilder.Append("-vf scale=1920:1080 ");
        commandBuilder.Append("-c:v h264_nvenc -rc vbr -cq 23 -pix_fmt yuv420p ");
        commandBuilder.Append("-c:a copy ");
        commandBuilder.Append("-map 0:v:0 -map 1:a:0 ");
        commandBuilder.Append($"\"{context.VideoFile}\" ");
        commandBuilder.Append(" -v info");
        return commandBuilder.ToString();
    }

    private static string GetImage(VideoGeneratorContext context)
    {
        if (context.Images == null || context.Images.Count == 0)
        {
            throw new ArgumentException("At least one image is required for video generation.", nameof(context));
        }

        if (context.Images.TryGetValue(context.Chapter, out NamedImageFile file))
        {
            return file.FilePath;
        }

        if (context.Images.TryGetValue(context.Book, out file))
        {
            return file.FilePath;
        }

        return null;
    }
}