using AudioBookStudio.Common.Media;
using AudioBookStudio.Common.Tests.Extensions;
using NAudio.Wave;
using System.Diagnostics;

namespace AudioBookStudio.Common.Tests.Services;
public class WavTransformTests
{
    [Fact]
    public async Task ConnectTwoWav_Output_Mp3()
    {
        var source = "Wave1.wav".GetResourceFile();
        var destination = "Wave1-volume.wav".GetResourceFile();
        var transformer = new WavTransformer();

        await transformer.AdjustVolume(source, destination, 2.0f);

        Assert.True(File.Exists(destination));
    }

    [Fact]
    public async Task ReadFile_With_Chunks()
    {
        var source = "AL1_3441.jpg".GetResourceFile();

        byte[] buffer = new byte[8192];
        int bytesRead;
        int totalBytesRead = 0;
        using var reader = new FileStream(source, FileMode.Open);
        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            totalBytesRead += bytesRead;
            Debug.WriteLine($"Bytes read: {bytesRead}, Position: {reader.Position}, Total: {totalBytesRead}");
        }
    }

    [Fact]
    public void Increase_wav_volume_test()
    {
        var source = "Wave1.wav".GetResourceFile();
        var destination = "Wave1_volume_2.wav".GetResourceFile();

        using (var audioFile = new AudioFileReader(source))
        {
            // Multiply the amplitude of each sample by a factor
            var volume = 4.0f;
            var volumeProvider = new MultiplyByConstantVolumeProvider(audioFile, volume);

            // Create an output WAV file with the increased volume
            using (var output = new WaveFileWriter(destination, volumeProvider.WaveFormat))
            {
                var buffer = new float[4096];
                int bytesRead;
                do
                {
                    bytesRead = volumeProvider.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < bytesRead; i++)
                    {
                        buffer[i] *= volume;
                    }
                    output.WriteSamples(buffer, 0, bytesRead);
                } while (bytesRead > 0);
            }
        }

        Assert.True(File.Exists(destination));
    }
}
