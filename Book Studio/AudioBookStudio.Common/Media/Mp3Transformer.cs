using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Diagnostics;

namespace AudioBookStudio.Common.Media;

public class Mp3Transformer
{
    public Task AdjustVolume(string source, string destination, int bitRate = 192000, float volume = 2.0f)
    {
        var temp = Path.GetTempFileName();

        try
        {
            using var audioFile = new Mp3FileReader(source);
            var sampleProvider = audioFile.ToSampleProvider();
            var volumeProvider = new VolumeSampleProvider(sampleProvider);
            volumeProvider.Volume = volume;

            // Create an output MP3 file with the increased volume
            using (var output = new LameMP3FileWriter(temp, sampleProvider.WaveFormat, bitRate))
            {
                var buffer = new float[8192];
                int bytesRead;
                do
                {
                    bytesRead = volumeProvider.Read(buffer, 0, buffer.Length);
                    var bytes = Convert(buffer);
                    output.Write(bytes, 0, bytes.Length);
                } while (bytesRead > 0);
            }

            File.Copy(temp, destination, true);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        finally
        {
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
        }

        return Task.CompletedTask;
    }

    private byte[] Convert(float[] input)
    {
        byte[] output = new byte[input.Length * 4];

        for (int i = 0; i < input.Length; i++)
        {
            byte[] bytes = BitConverter.GetBytes(input[i]);
            Array.Copy(bytes, 0, output, i * 4, 4);
        }

        return output;
    }
}

