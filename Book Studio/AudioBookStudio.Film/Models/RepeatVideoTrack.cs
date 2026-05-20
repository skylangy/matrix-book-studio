using AudioBookStudio.Films.Common;

namespace AudioBookStudio.Films.Models;
public class RepeatVideoTrack : Track
{
    public string? AudioPath { get; set; }
    public string? VideoPath { get; set; }
    public Size Size { get; set; } = Size.Empty;
    public double FadeIn { get; set; } = 0.0;
    public double FadeOut { get; set; } = 0.0;

    private MediaFile? _audioFile;


    public RepeatVideoTrack()
    {
        HasInput = true;
    }

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        return ["-stream_loop", "-1", "-i", VideoPath.QuotePath()];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        _audioFile = new MediaFile(AudioPath!);

        var size = Size.IsEmpty ? context?.Settings?.Resolution : Size;
        var duration = _audioFile.GetDuration();
        var filters = new FilterList();

        if (size != null)
            filters.Add($"scale={size.Width}:{size.Height}");

        if (Alpha < 1.0)
            filters.Add("format=rgba,colorchannelmixer=aa=" + Alpha.ToString("0.##"));
        else if (context != null)
            filters.Add($"format={context.Settings.PixelFormat}");

        if (FadeIn > 0)
            filters.Add($"fade=t=in:st=0:d={FadeIn}");
        if (FadeOut > 0 && duration > 0)
            filters.Add($"fade=t=out:st={duration - FadeOut}:d={FadeOut}");

        var filterStr = string.Join(",", filters);

        Filter.InputIndex = context?.InputIndex ?? 0;
        Filter.InputLabel = $"[{(context?.InputIndex ?? 0)}:v]";
        Filter.OutputLabel = $"[v{(context?.InputIndex ?? 0)}]";
        Filter.Filters = [filterStr];

        context?.ConnectTracks?.Add(this);
    }
}
