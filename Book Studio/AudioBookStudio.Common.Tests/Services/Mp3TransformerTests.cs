using AudioBookStudio.Common.Media;
using AudioBookStudio.Common.Tests.Extensions;

namespace AudioBookStudio.Common.Tests.Services;

public class Mp3TransformerTests
{
    [Fact]
    public async Task ConnectTwoWav_Output_Mp3()
    {
        var source = "Audio.mp3".GetResourceFile();
        var destination = "Audio-volume.mp3".GetResourceFile();
        var transformer = new Mp3Transformer();

        await transformer.AdjustVolume(source, destination);

        Assert.True(File.Exists(destination));
    }

}

