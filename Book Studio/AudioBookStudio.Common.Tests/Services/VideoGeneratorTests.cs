using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;
using System.Text;

namespace AudioBookStudio.Common.Tests.Services;

public class VideoGeneratorTests
{
    [Fact]
    public async Task GenerateVideo_From_Images_Mp3()
    {
        var context = new VideoGeneratorContext()
        {
            AudioFile = GetResourceFile("Audio.mp3"),
            VideoFile = GetResourceFile("output.mp4"),
        };

        Assert.True(File.Exists(context.AudioFile));


        var appConfig = Mock.Of<IOptions<AppConfiguration>>(_ => _.Value == new AppConfiguration
        {
            UseGpu = false,
            RegexLibDatabasePath = "RegexLib.db",
            BooksLocation = "Books",
            DbUrl = "https://localhost:5001",
            DbName = "BookStudio",
            BookDatabasePath = "Books.db",
            OptionDatabasePath = "Options.db",
            Synthesizer = "DefaultSynthesizer" // Added required property
        });
        var commandInvoker = Mock.Of<ICommandInvoker>();
        var logger = Mock.Of<ILogger<VideoGenerator>>();
        var generator = new VideoGenerator(appConfig, commandInvoker, logger);
        var result = await generator.Generate(context);

        Assert.True(File.Exists(context.VideoFile));
    }

    [Fact]
    public void FFMpeg_Command_line()
    {
        var context = new VideoGeneratorContext()
        {
            AudioFile = GetResourceFile("Audio.mp3"),
            VideoFile = GetResourceFile("output2.mp4"),
        };

        var audioFile = TagLib.File.Create(context.AudioFile);
        var audioDuration = audioFile.Properties.Duration.TotalSeconds;

        var images = string.Join(' ', context.Images.Select(x => $"-loop 1 -i {x}"));
        var metadata = $@"-metadata title=""test video"" ";

        var commandBuilder = new StringBuilder();
        commandBuilder.Append(@$"-r 1 ");
        commandBuilder.Append(@$"-loop 1 ");
        commandBuilder.Append(@$"-y ");
        commandBuilder.Append(@$"-i ""{context.Images.First()}"" ");
        commandBuilder.Append(@$"-i ""{context.AudioFile}"" ");
        commandBuilder.Append($@"-metadata title=""{context.MetadataTitle}"" ");
        commandBuilder.Append("-tune stillimage ");
        commandBuilder.Append("-vf scale=1280x720 ");
        commandBuilder.Append(@$"-c:a copy ");
        commandBuilder.Append("-c:v libx264 -crf 18 -pix_fmt yuv420p ");
        commandBuilder.Append("-shortest ");
        commandBuilder.Append(@$"-r 1 ");
        commandBuilder.Append(@$"""{context.VideoFile}"" ");
        var command = commandBuilder.ToString();

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"{command}",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        var start = DateTime.Now;
        var process = new Process
        {
            StartInfo = startInfo
        };
        process.Start();

        process.WaitForExit();

        var span = DateTime.Now - start;
        Debug.WriteLine($"Export video takes {span:hh\\:mm\\:ss}");

        Assert.True(File.Exists(context.VideoFile));

    }

    private static string GetResourceFile(string name)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", name);
    }
}