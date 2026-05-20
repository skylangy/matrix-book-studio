namespace AudioBookStudio.Films.Models;
public class TextGroupTrack : Track
{
    public IList<TextModel> Texts { get; set; } = [];
    public Align Align { get; set; } = Align.Center;
    public Margin Margin { get; set; } = new(0, 0, 0, 0);
    public int LineSpacing { get; set; } = 0;
    public double FadeInStart { get; set; }
    public double FadeInDuration { get; set; } = 0.0;

    public TextGroupTrack()
    {
        HasInput = false;
    }

    public override FilterList BuildInputs(CommandBuilderContext context)
    {
        return [];
    }

    public override void BuildFilter(CommandBuilderContext context)
    {
        var resultLabel = $"[tg{(context?.InputIndex ?? 0)}]";
        var textSizes = CalculateTextSize();
        int maxWidth = textSizes.Values.Count > 0 ? textSizes.Values.Max(s => s.Width) : 0;
        var margin = Margin != null ? Margin.Copy() : Margin.Empty;
        var filters = new FilterList();

        for (int i = 0; i < Texts.Count; i++)
        {
            var textModel = Texts[i];
            var fontFile = Fonts.Resolve(textModel.FontName)?.EscapePath() ?? string.Empty;
            var size = textSizes.TryGetValue(i, out Size? value) ? value : Size.Empty;
            var location = Alignment.GetLocation(Align, context?.Settings?.Resolution, size, margin, maxWidth);

            margin.Top += size.Height + LineSpacing;

            var modelFilters = new FilterList
                {
                    $"drawtext=fontfile='{fontFile}'",
                    $"text=\"{textModel.EscapeText()}\"",
                    $"fontsize={textModel.FontSize}"
                };

            if (textModel.Alpha < 1.0)
                modelFilters.Add($"fontcolor={textModel.FontColor}@{textModel.Alpha}");
            else
                modelFilters.Add($"fontcolor={textModel.FontColor}");

            modelFilters.Add($"x={location.X}");
            modelFilters.Add($"y={location.Y}");

            if (textModel.ShowShadow)
            {
                modelFilters.Add($"shadowcolor={textModel.ShadowColor}");
                modelFilters.Add($"shadowx={textModel.ShadowX}");
                modelFilters.Add($"shadowy={textModel.ShadowY}");
            }

            if (Start > 0)
            {
                modelFilters.Add($"enable='between(t,{Start},{Duration})'");
            }

            if (FadeInDuration > 0)
            {
                modelFilters.Add($"fade=t=in:st={FadeInStart}:d={FadeInDuration}");
            }


            filters.Add(modelFilters.Join(":"));
        }

        Filter.InputIndex = context.InputIndex;
        Filter.OutputLabel = resultLabel;
        Filter.InputLabel = $"[{context.InputIndex}:v]";
        Filter.Filters = [filters.Join(",")];

        context.CurrentLabel = resultLabel;
        context.OverlaysTracks?.Add(this);
    }

    private Dictionary<int, Size> CalculateTextSize()
    {
        var textSizes = new Dictionary<int, Size>();
        for (int i = 0; i < Texts.Count; i++)
        {
            textSizes[i] = Texts[i].CalculateSize();
        }
        return textSizes;
    }

    public override string ToString()
    {
        return $"TextGroup: {string.Join(", ", Texts.Select(t => t.Text))}";
    }
}
