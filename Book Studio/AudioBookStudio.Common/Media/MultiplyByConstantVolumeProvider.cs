using NAudio.Wave;

namespace AudioBookStudio.Common.Media;

public class MultiplyByConstantVolumeProvider : ISampleProvider
{
    private readonly ISampleProvider sourceProvider;
    private readonly float volume;

    public MultiplyByConstantVolumeProvider(ISampleProvider sourceProvider, float volume)
    {
        this.sourceProvider = sourceProvider;
        this.volume = volume;
    }

    public WaveFormat WaveFormat => sourceProvider.WaveFormat;

    public float Volume => volume;

    public int Read(float[] buffer, int offset, int count)
    {
        var samplesRead = sourceProvider.Read(buffer, offset, count);

        for (int i = 0; i < samplesRead; i++)
        {
            buffer[offset + i] *= volume;
        }

        return samplesRead;
    }
}

