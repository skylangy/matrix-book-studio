namespace AudioBookStudio.Films.Models;
public class VideoOverlayTrack : OverlayTrack
{
    public string? Path { get; set; }
    public Location Location { get; set; } = new Location(0, 0);
    public Size Size { get; set; } = Size.Empty;

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        FilterList inputs = ["-stream_loop", "-1", "-i", Path!];

        if (Duration > 0)
        {
            inputs.Add("-t");
            inputs.Add(Duration.ToString());
        }
        return inputs;
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        var location = Location;
        var size = Size ?? context?.Settings?.Resolution;
        var resultLabel = $"[ov{(context?.InputIndex ?? 0)}]";
        var intermediateLabel = $"[ov{(context?.InputIndex ?? 0)}_i]";
        var filters = new FilterList { "format=rgba" };
        if (Alpha < 1.0)
            filters.Add($"colorchannelmixer=aa={Alpha:0.##}");
        if (size != null)
            filters.Add($"scale={size.Width}:{size.Height}");


        Filter.InputIndex = context.InputIndex;
        Filter.OutputLabel = $"[ov{context.InputIndex}_v]";
        Filter.InputLabel = $"[{context.InputIndex}:v]";
        Filter.Filters = [
                    $"{string.Join(",", filters)}{intermediateLabel}",
                    $"{{ctx.CurrentLabel}}{intermediateLabel}overlay={location.X}:{location.Y}"
                ];

        context.CurrentLabel = resultLabel;
        context.OverlaysTracks?.Add(this);

    }

    public override string ToString()
    {
        return $"VideoOverlayTrack: {Path} {Filter}";
    }
}
