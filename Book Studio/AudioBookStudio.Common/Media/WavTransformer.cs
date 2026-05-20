
using NAudio.Wave;
using System.Diagnostics;

namespace AudioBookStudio.Common.Media;

public class WavTransformer
{
    public Task AdjustVolume(string source, string destination, float volume = 2.0f)
    {
        var temp = Path.GetTempFileName();

        try
        {
            using (var audioFile = new AudioFileReader(source))
            {
                var volumeProvider = new MultiplyByConstantVolumeProvider(audioFile, volume);

                // Create an output WAV file with the increased volume
                using (var output = new WaveFileWriter(temp, volumeProvider.WaveFormat))
                {
                    var buffer = new float[8192];
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

                File.Copy(temp, destination, true);
            }
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
}

