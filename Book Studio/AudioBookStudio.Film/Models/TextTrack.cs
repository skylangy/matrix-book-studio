namespace AudioBookStudio.Films.Models;
public class TextTrack : Track
{
    public TextModel Text { get; set; } = new TextModel("");
    public Location Location { get; set; } = new Location(0, 0);

    public TextTrack()
    {
        HasInput = false;
    }

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        return [];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        string fontFile = Fonts.Resolve(Text.FontName)?.EscapePath() ?? "";


        var filters = new FilterList()
            {
                $"drawtext=fontfile=\"{fontFile}\":",
                $"text=\"{Text.EscapeText()}\":",
                $"fontsize={Text.FontSize}:",
                $"x={Location.X}:",
                $"y={Location.Y}:",
            };
        if (Text.Alpha < 1.0)
            filters.Add($"fontcolor={Text.FontColor}@{Text.Alpha}:");
        else
            filters.Add($"fontcolor={Text.FontColor}:");

        if (Text.ShowShadow)
        {
            filters.Add($"shadowx={Text.ShadowX}:");
            filters.Add($"shadowy={Text.ShadowY}:");
            filters.Add($"shadowcolor={Text.ShadowColor}:");
        }

        Filter.InputIndex = context.InputIndex;
        Filter.OutputLabel = $"[t{context.InputIndex}]";
        Filter.InputLabel = $"[{context.InputIndex}:v]";
        Filter.Filters = filters;

        context?.OverlaysTracks?.Add(this);
    }

    public override string ToString()
    {
        return $"Text: {Text.Text}";
    }
}
