using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.IO;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace AudioBookStudio.Common.Services;

public record VideoCombineInfo(string Name, string ItemsFile);


public class VideoCombiner(
    IOptions<VideoCombineConfig> videoCombineConfig,
    ICommandInvoker commandInvoker,
    IPathMappingService pathMappingService,
    ILogger<VideoCombiner> logger) : IVideoCombiner
{
    private const string FFMpeg = "ffmpeg";
    private readonly TimeSpan MaxDuration = TimeSpan.FromHours(10);
    private readonly VideoCombineConfig _config = videoCombineConfig.Value;
    private readonly ICommandInvoker _commandInvoker = commandInvoker;
    private readonly IPathMappingService _pathMappingService = pathMappingService;
    private readonly ILogger<VideoCombiner> _logger = logger;

    public async Task<VideoCombineResult> Combine(VideoCombineContext context)
    {
        var result = new VideoCombineResult();

        try
        {
            var start = DateTime.Now;

            var combineInfos = Prepare(context);

            foreach (var combineInfo in combineInfos)
            {
                _logger.LogInformation("Combining videos for {Name} with {Count} source files", combineInfo.Name, combineInfo.ItemsFile);

                var output = Path.Combine(context.OutputFolder, $"{combineInfo.Name}.mp4");
                var cmdParameters = BuildCommandParameters(output, combineInfo.ItemsFile);
                var command = new CommandModel { Arguments = cmdParameters, Command = FFMpeg };

                _logger.LogInformation("Combining videos with command: {name} {Command} {Parameters}", combineInfo.Name, FFMpeg, cmdParameters);

                await _commandInvoker.InvokeAsync(command);
            }

            await ProcessSubtitles(combineInfos, context);

            result.Duration = DateTime.Now - start;
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error combining videos: {message}", ex.FullMessage());
            result.Error = ex.Message;
            result.Success = false;
        }

        return result;
    }

    private List<VideoCombineInfo> Prepare(VideoCombineContext context)
    {
        _logger.LogInformation("Preparing video combine for {Name} with {Count} source files", context.Name, context.SourceFiles.Count());

        var result = new List<VideoCombineInfo>();
        TimeSpan totalDuration = TimeSpan.Zero;
        int startIndex = 1;
        var files = new List<string>();

        var sourceFiles = context.SourceFiles;
        if (_config.SkipChapters > 0 && sourceFiles.Count() > _config.SkipChapters)
        {
            sourceFiles = sourceFiles.Take(sourceFiles.Count() - _config.SkipChapters);
        }

        foreach (var sourceFile in sourceFiles)
        {
            var windowsPath = _pathMappingService.Map(sourceFile).ToWindowsPath();
            var videoInfo = new VideoInfo(sourceFile);
            totalDuration += videoInfo.Duration;

            if (totalDuration > MaxDuration)
            {
                AddCombineInfo(result, context, ref startIndex, files, false);
                totalDuration = videoInfo.Duration;
            }

            files.Add(windowsPath);
        }

        if (files.Count > 0)
        {
            bool isSingle = result.Count == 0; // Only one output file if result is empty before adding
            AddCombineInfo(result, context, ref startIndex, files, isSingle);
        }

        return result;
    }

    private static void AddCombineInfo(List<VideoCombineInfo> result, VideoCombineContext context, ref int startIndex, List<string> files, bool isSingle)
    {
        string name = isSingle ? context.Name : $"{context.Name}-{startIndex}";
        var combineInfo = new VideoCombineInfo(name, Path.Combine(context.OutputFolder, $"{name}.txt"));
        var content = string.Join("", files.Select(x => $"file '{x}'\n"));
        File.WriteAllText(combineInfo.ItemsFile, content);
        result.Add(combineInfo);

        files.Clear();
        startIndex++;
    }

    private string BuildCommandParameters(string outputFile, string tempFile)
    {
        var destFile = _pathMappingService.Map(outputFile).ToWindowsPath();
        var itemsFile = _pathMappingService.Map(tempFile).ToWindowsPath();
        var builder = new StringBuilder();
        builder.Append($"-f concat -safe 0 -i \"{itemsFile}\" -c copy \"{destFile}\"");
        return builder.ToString();
    }

    private async Task ProcessSubtitles(IEnumerable<VideoCombineInfo> videoCombineInfos, VideoCombineContext context)
    {
        foreach (var videoCombineInfo in videoCombineInfos)
        {
            var itemsFile = videoCombineInfo.ItemsFile;
            var lines = await File.ReadAllLinesAsync(itemsFile);

            var srtFiles = ParseSrtFiles(lines);
            var output = Path.Combine(context.OutputFolder, $"{videoCombineInfo.Name}.srt");
            CombineSrtFiles(srtFiles, output);
        }
    }

    private List<SrtFile> ParseSrtFiles(IEnumerable<string> lines)
    {
        var srtFiles = new List<SrtFile>();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var filePath = line.Split('\'')[1];
            var srtFile = filePath.Replace("mp4", "srt");
            if (!File.Exists(srtFile))
            {
                _logger.LogWarning("SRT file not found for {filePath}", filePath);
                srtFiles.Clear();
                break;
            }

            srtFiles.Add(SrtFile.Load(srtFile));
        }
        return srtFiles;
    }

    private static void CombineSrtFiles(IEnumerable<SrtFile> srtFiles, string outputFile)
    {
        if (!srtFiles.Any())
            return;

        var first = srtFiles.First();
        var offset = first.Lines.Last().End.Add(TimeSpan.FromSeconds(1));

        foreach (var srtFile in srtFiles.Skip(1))
        {
            srtFile.ShiftAll(offset);
            first.Lines.AddRange(srtFile.Lines);
            offset = first.Lines.Last().End.Add(TimeSpan.FromSeconds(1.2));
        }

        first.Save(outputFile);
    }
}
