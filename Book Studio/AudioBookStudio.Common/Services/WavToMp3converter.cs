using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.Media;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NAudio.Lame;
using NAudio.Wave;
using System.Text;

namespace AudioBookStudio.Common.Services;

public sealed class WavToMp3Converter(
       IOptions<AppConfiguration> appConfig,
       ICommandInvoker commandInvoker,
       IPathMappingService pathMappingService,
       ILogger<WavToMp3Converter> logger) : IWavToMp3Converter
{
    private readonly AppConfiguration _appConfig = appConfig.Value;
    private readonly Mp3Transformer _transformer = new();
    private readonly ICommandInvoker _commandInvoker = commandInvoker;
    private readonly IPathMappingService _pathMappingService = pathMappingService;
    private readonly ILogger<WavToMp3Converter> _logger = logger;
    private const bool UseFfmpeg = true;
    private const string FFMpeg = "ffmpeg";

    public async Task<ConvertResult> Convert(ConvertContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        return UseFfmpeg ? await ConvertWithFfmpeg(context) : await ConvertWithLame(context);
    }

    public async Task<ConvertResult> Enhance(ConvertContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        var result = new ConvertResult();
        try
        {
            foreach (var source in context.Sources)
            {
                var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp3");
                var arguments = BuildEnhanceCommandParameters(source, tempFile);
                var command = new CommandModel
                {
                    Arguments = arguments,
                    Command = FFMpeg
                };
                await _commandInvoker.InvokeAsync(command);

                File.Copy(tempFile, source, true);
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            context.ExceptionHandler?.Invoke(ex);
            result.Success = false;
            result.Exception = ex;
        }
        return result;
    }

    public async Task<ConvertResult> ConvertWithFfmpeg(ConvertContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var result = new ConvertResult();
        var start = DateTime.Now;
        var concatFile = string.Empty;
        try
        {
            _logger.LogInformation("Starting FFMpeg WAV to MP3 conversion");
            var tempDir = Path.Combine(_appConfig.BooksLocation, "temp");
            _logger.LogInformation("Temp folder {folder}", tempDir);
            tempDir.EnsureDirectoryExists();

            concatFile = Path.Combine(tempDir, $"{context.Book}-{context.Chapter}.txt");

            if (context.IsValid())
            {
                using (var writer = new StreamWriter(concatFile))
                {
                    foreach (var source in context.Sources)
                    {
                        var path = _pathMappingService.Map(source).ToWindowsPath();
                        writer.WriteLine($"file '{path}'");
                    }
                }

                string arguments = BuildCommandParameters(context, concatFile);

                var command = new CommandModel
                {
                    Arguments = arguments,
                    Command = FFMpeg
                };
                _logger.LogInformation("Command arguments: {}", arguments);

                await _commandInvoker.InvokeAsync(command);
                result.Success = true;
                result.Duration = DateTime.Now - start;
            }
            else
            {
                result.Success = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting WAV to MP3 {message}", ex.FullMessage());
            context.ExceptionHandler?.Invoke(ex);
            result.Success = false;
            result.Exception = ex;
        }
        return result;
    }

    private async Task<ConvertResult> ConvertWithLame(ConvertContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var result = new ConvertResult();
        var tempFile = Path.GetTempFileName();
        try
        {
            if (context.IsValid())
            {
                var start = DateTime.Now;

                using var wavReader = new AudioFileReader(context.Sources.FirstOrDefault());
                using var outputStream = new FileStream(tempFile, FileMode.Create);
                using var writer = new LameMP3FileWriter(outputStream, wavReader.WaveFormat, context.BitRate);
                foreach (var source in context.Sources)
                {
                    using var stream = new AudioFileReader(source);
                    await stream.CopyToAsync(writer);
                }
                writer.Close();
                outputStream.Close();

                await _transformer.AdjustVolume(tempFile, context.Destination, context.BitRate, 2);

                result.Success = true;
                result.Duration = DateTime.Now - start;
            }
            else
            {
                result.Success = false;
            }
        }
        catch (Exception ex)
        {
            context.ExceptionHandler?.Invoke(ex);

            result.Success = false;
            result.Exception = ex;
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        return result; ;
    }

    private string BuildCommandParameters(ConvertContext context, string concatFile)
    {
        var destination = _pathMappingService.Map(context.Destination);
        var itemsFile = _pathMappingService.Map(concatFile).ToWindowsPath();

        var commandBuilder = new StringBuilder();
        commandBuilder.Append("-y ");
        commandBuilder.Append($"-f concat -safe 0 -i \"{itemsFile}\" ");    // Concatenate WAV files
        commandBuilder.Append($"-c:a libmp3lame -b:a {context.BitRate} ");  // Encode to MP3
        commandBuilder.Append("-af volume=4 ");                             // Increase volume by 2x
        commandBuilder.Append($"\"{destination}\" ");                       // Output MP3 file

        return commandBuilder.ToString();
    }

    private string BuildEnhanceCommandParameters(string sourceFile, string output, float volume = 4f)
    {
        var commandBuilder = new StringBuilder();
        commandBuilder.Append("-y ");
        commandBuilder.Append($"-i \"{sourceFile}\" ");
        commandBuilder.Append("-c:a libmp3lame -b:a 192000 ");
        commandBuilder.Append($"-af volume={volume} ");
        commandBuilder.Append($"\"{output}\"");
        return commandBuilder.ToString();
    }
}