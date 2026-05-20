using System.Collections.Generic;

namespace FilmCS
{
    public class TextTrack : Track
    {
        public TextModel Text { get; set; } = new TextModel("");
        public Location Location { get; set; } = new Location(0, 0);

        public TextTrack()
        {
            HasInput = false;
        }

        public override List<string> GetInputs(CommandBuilderContext context)
        {
            return new List<string>();
        }

        public override object GetFilter(CommandBuilderContext context)
        {
            string fontFile = Fonts.Resolve(Text.FontName)?.EscapePath() ?? "";
            var filters = new List<string>
            {
                $"drawtext=fontfile=\"{fontFile}\":",
                $"text=\"{Text.Text.Replace(":", "\\:").Replace("'", "\\'")}\":",
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
            if (FilterInfo != null && context != null)
            {
                FilterInfo.InputIndex = context.InputIndex;
                FilterInfo.OutputLabel = $"[t{context.InputIndex}]";
                FilterInfo.InputLabel = $"[{context.InputIndex}:v]";
                FilterInfo.Filters = filters;
            }
            context?.OverlaysTracks?.Add(this);
            return new FilterResult("", FilterInfo?.OutputLabel);
        }

        public override string ToString()
        {
            return $"Text: {Text.Text}";
        }
    }
}
