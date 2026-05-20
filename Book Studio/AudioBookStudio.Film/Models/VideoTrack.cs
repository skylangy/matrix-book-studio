namespace AudioBookStudio.Films.Models;
public class VideoTrack : Track
{
    public string? Path { get; set; }
    public Location Location { get; set; } = new Location(0, 0);
    public Size Size { get; set; } = Size.Empty;
    public double FadeIn { get; set; } = 0.0;
    public double FadeOut { get; set; } = 0.0;

    public VideoTrack()
    {

    }

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        if (Duration == 0)
            return ["-stream_loop", "-1", "-i", Path];

        var videoFile = new MediaFile(Path!);
        double duration = videoFile.GetDuration();
        context.Settings.Duration += duration + (HasTransition ? TransitionDuration : 0);

        return ["ss", Start.ToString(), "-t", Duration.ToString(), "-i", $"\"{Path}\""];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        var videoFile = new MediaFile(Path!);

        var size = Size ?? context?.Settings?.Resolution;
        var duration = videoFile.GetDuration();
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


        Filter.InputIndex = context?.InputIndex ?? 0;
        Filter.InputLabel = $"[{(context?.InputIndex ?? 0)}:v]";
        Filter.OutputLabel = $"[v{(context?.InputIndex ?? 0)}]";
        Filter.Filters = filters;

        context?.ConnectTracks?.Add(this);
    }

    public override string ToString()
    {
        return $"VideoTrack: {Path} {Filter}";
    }
}
