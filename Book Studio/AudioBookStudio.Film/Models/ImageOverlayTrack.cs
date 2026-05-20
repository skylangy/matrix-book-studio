namespace AudioBookStudio.Films.Models;
public class ImageOverlayTrack : OverlayTrack
{
    public string? Path { get; set; }
    public Margin Margin { get; set; } = new Margin(0, 0, 0, 0);
    public Align Align { get; set; } = Align.Right;
    public Size Size { get; set; } = new Size(64, 64);

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        return ["-loop", "1", "-i", Path];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        var size = Size ?? context?.Settings?.Resolution;
        var location = Alignment.GetLocation(Align, context?.Settings?.Resolution, size, Margin);

        var intermediateLabel = $"[ov{(context?.InputIndex ?? 0)}_i]";
        var filters = new FilterList();

        if (Alpha < 1.0)
            filters.Add($"format=rgba,colorchannelmixer=aa={Alpha:0.##}");
        if (size != null)
            filters.Add($"scale={size.Width}:{size.Height}");

        Filter.InputIndex = context.InputIndex;
        Filter.InputLabel = $"[{context.InputIndex}:v]";
        Filter.OutputLabel = $"[ov{(context?.InputIndex ?? 0)}]";
        Filter.Filters = [
                    $"{string.Join(",", filters)}{intermediateLabel}",
                    $"{{ctx.CurrentLabel}}{intermediateLabel}overlay=x={location.X}:y={location.Y}"
                ];

        context.CurrentLabel = Filter.OutputLabel;
        context.OverlaysTracks?.Add(this);
    }

    public override string ToString()
    {
        return $"ImageOverlay: {Path} {Filter}";
    }
}
