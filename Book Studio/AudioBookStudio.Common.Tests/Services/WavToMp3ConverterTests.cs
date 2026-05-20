using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NAudio.Lame;
using NAudio.Wave;

namespace AudioBookStudio.Common.Tests.Services;

public class WavToMp3ConverterTests
{
    [Fact]
    public async Task ConnectTwoWav_Output_Mp3()
    {
        var sources = new string[] { GetResourceFile("Wave1.wav"), GetResourceFile("Wave2.wav") };
        var destination = GetResourceFile("output.mp3");
        var mockAppConfig = new Mock<IOptions<AppConfiguration>>();
        var mockCommandInvoker = new Mock<ICommandInvoker>();
        var mockPathMappingService = new Mock<IPathMappingService>();

        var converter = new WavToMp3Converter(
               mockAppConfig.Object,
               mockCommandInvoker.Object,
               mockPathMappingService.Object,
               NullLogger<WavToMp3Converter>.Instance);

        var context = new ConvertContext { Sources = sources, Destination = destination };
        var result = await converter.Convert(context);

        Assert.True(result.Success);
        Assert.Null(result.Exception);
        Assert.True(File.Exists(destination));
    }

    [Fact]
    public void Convert_WAV_To_MP3()
    {
        int bitRate = 192;
        string output = GetResourceFile("Wave1.mp3");
        using (var reader = new AudioFileReader(GetResourceFile("Wave1.wav")))
        using (var writer = new LameMP3FileWriter(output, reader.WaveFormat, bitRate))
            reader.CopyTo(writer);

        Assert.True(File.Exists(output));

        var fileInfo = new FileInfo(output);
        Assert.True(fileInfo.Length > 0);

    }

    private static string GetResourceFile(string name)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", name);
    }
}