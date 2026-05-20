using System.Collections.Generic;
using System.Linq;

namespace FilmCS
{
    public class TextGroupTrack : Track
    {
        public List<TextModel> Texts { get; set; } = new List<TextModel>();
        public Align Align { get; set; } = Align.Center;
        public Margin Margin { get; set; } = new Margin(0, 0, 0, 0);
        public int LineSpacing { get; set; } = 10;
        public double Alpha { get; set; } = 1.0;

        public TextGroupTrack()
        {
            HasInput = false;
        }

        public override List<string> GetInputs(object context)
        {
            return new List<string>();
        }

        public override object GetFilter(object context)
        {

            var resultLabel = $"[tg{(context?.InputIndex ?? 0)}]";
            var textSizes = CalculateTextSize();
            int maxWidth = textSizes.Values.Count > 0 ? textSizes.Values.Max(s => s.Width) : 0;
            var margin = Margin != null ? new Margin(Margin.Left, Margin.Right, Margin.Top, Margin.Bottom) : new Margin();
            var filters = new List<string>();
            for (int i = 0; i < Texts.Count; i++)
            {
                var textModel = Texts[i];
                string fontFile = Fonts.Resolve(textModel.FontName)?.EscapePath() ?? "";
                var size = textSizes.ContainsKey(i) ? textSizes[i] : new Size(0, 0);
                var location = Alignment.GetLocation(Align, context?.Settings?.Resolution, size, margin, maxWidth);
                var modelFilters = new List<string>
                {
                    $"drawtext=fontfile='{fontFile}'",
                    $"text='{textModel.Text.Replace(":", "\\:").Replace("'", "\\'")}'",
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
                string modelFilter = string.Join(":", modelFilters);
                margin.Top += size.Height + LineSpacing;
                filters.Add(modelFilter);
            }
            string filter = string.Join(",", filters);
            if (FilterInfo != null && context != null)
            {
                FilterInfo.InputIndex = context.InputIndex;
                FilterInfo.OutputLabel = resultLabel;
                FilterInfo.InputLabel = $"[{context.InputIndex}:v]";
                FilterInfo.Filters = new List<string> { filter };
            }
            if (context != null)
            {
                context.CurrentLabel = resultLabel;
                context.OverlaysTracks?.Add(this);
                context.Logger?.Info($"TextGroupTrack filter: {filter}, current label: {resultLabel}");
            }
            return new FilterResult("", resultLabel);
        }

        private Dictionary<int, Size> CalculateTextSize()
        {
            var textSizes = new Dictionary<int, Size>();
            for (int i = 0; i < Texts.Count; i++)
            {
                // In real code, use font metrics to get text size
                textSizes[i] = new Size(100, 30); // Placeholder
            }
            return textSizes;
        }

        public override string ToString()
        {
            return $"TextGroup: {string.Join(", ", Texts.Select(t => t.Text))}";
        }
    }
}
