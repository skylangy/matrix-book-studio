namespace AudioBookStudio.Films.Models;
public class ImageTrack : Track
{
    public required string Path { get; set; }
    public Location Position { get; set; } = new Location(0, 0);
    public Size? Size { get; set; }
    public double FadeInStart { get; set; }
    public double FadeInDuration { get; set; } = 0.0;
    public double FadeOutStart { get; set; }
    public double FadeOutDuration { get; set; } = 0.0;

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        double duration = Duration; // + TransitionDuration;

        context.Settings.Duration += duration;

        return ["-loop", "1", "-t", duration.ToString(), "-i", $"\"{Path}\""];
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
        if (FadeOutDuration > 0)
        {
            filters.Add($"fade=t=out:st={FadeOutStart}:d={FadeOutDuration}");
        }


        Filter.InputIndex = context.InputIndex;
        Filter.InputLabel = $"[{context.InputIndex}:v]";
        Filter.OutputLabel = $"[v{context.InputIndex}]";
        Filter.Filters = filters;

        context.CurrentLabel = $"[v{context.InputIndex}]";
        context.ConnectTracks?.Add(this);
    }

    public override string ToString()
    {
        return $"ImageTrack: {Path}";
    }
}
