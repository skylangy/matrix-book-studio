using AudioBookStudio.Films.Common;

namespace AudioBookStudio.Films.Models;
public class ImageAudioTrack : Track
{
    public required string ImagePath { get; set; }
    public required string AudioPath { get; set; }
    public Location Position { get; set; } = new Location(0, 0);
    public Size? Size { get; set; }
    public double FadeInStart { get; set; }
    public double FadeInDuration { get; set; } = 0.0;

    private MediaFile? _audioFile;

    public ImageAudioTrack()
    {
        HasInput = true;
    }

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        _audioFile = new MediaFile(AudioPath!);

        double duration = _audioFile.GetDuration();

        context.Settings.Duration += duration - FadeInDuration;

        return ["-loop", "1", "-t", duration.ToString(), "-i", ImagePath.QuotePath()];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        var size = Size ?? context?.Settings?.Resolution;
        var filters = new FilterList();

        if (size != null)
            filters.Add($"scale={size.Width}:{size.Height}");

        if (Alpha < 1.0)
            filters.Add($"format=rgba,colorchannelmixer=aa={Alpha:0.##}");
        else if (context != null)
            filters.Add($"format={context.Settings.PixelFormat}");

        if (FadeInDuration > 0)
        {
            filters.Add($"fade=t=in:st={FadeInStart}:d={FadeInDuration}");
        }


        Filter.InputIndex = context.InputIndex;
        Filter.InputLabel = $"[{context.InputIndex}:v]";
        Filter.OutputLabel = $"[v{context.InputIndex}]";
        Filter.Filters = filters;

        context.CurrentLabel = $"[v{context.InputIndex}]";
        context.ConnectTracks?.Add(this);
    }
}
