using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.IO;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services;
public class TimeSubtitleGenerator(ILogger<TimeSubtitleGenerator> logger) : ISubtitleGenerator
{
    public async Task GenerateSubtitlesAsync(SubtitleGenerateContext context)
    {
        logger.LogInformation("Generate subtitle for {}", context.Chapter?.Title);

        await GenerateSubtitleAsync(context);
    }

    public async Task GenerateSubtitlesFromFolderAsync(SubtitleGenerateContext context)
    {
        logger.LogInformation("Generate subtitle for {} to {}", context.AudioFilePath, context.SrtFolder);

        var files = Directory.GetFiles(context.AudioFolder, $"*.{ResourceTypes.Mp3}");
        foreach (var file in files)
        {
            var audioFileName = Path.GetFileNameWithoutExtension(file);
            var subtitleFilePath = Path.Combine(context.SrtFolder, $"{audioFileName}.{ResourceTypes.Srt}");
            var subtitleContext = new SubtitleGenerateContext
            {
                Chapter = context.Chapter,
                AudioFilePath = file,
                SubtitleFilePath = subtitleFilePath,
                AudioFolder = context.AudioFolder,
                SrtFolder = context.SrtFolder
            };
            await GenerateSubtitleAsync(subtitleContext);
        }
    }

    private async Task GenerateSubtitleAsync(SubtitleGenerateContext context)
    {
        logger.LogInformation("Generate subtitle for {Audio} to {Subtitle}", context.AudioFilePath, context.SubtitleFilePath);

        var mp3Info = new Mp3Info(context.AudioFilePath);
        await mp3Info.Load();
        var duration = mp3Info.Duration.TotalSeconds;

        var srtFile = new SrtFile();

        var lines = context.Chapter.Content.ToLines();
        var weights = lines.Select(l => l.GetWeightedLength()).ToList();
        var totalWeight = weights.Sum();

        int index = 1;
        double accumulatedTime = 0;

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            double proportion = weights[i] / totalWeight;
            double lineDuration = duration * proportion;

            double startSec = accumulatedTime;
            double endSec = accumulatedTime + lineDuration;

            if (i == lines.Count - 1)
                endSec = duration;

            srtFile.Lines.Add(new SrtLine
            {
                Index = index++,
                Start = TimeSpan.FromSeconds(startSec),
                End = TimeSpan.FromSeconds(endSec),
                TextLines = [line]
            });

            accumulatedTime = endSec;
        }

        srtFile.Save(context.SubtitleFilePath);
    }
}
