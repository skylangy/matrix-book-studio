using System.Collections.Generic;

namespace FilmCS
{
    public class ImageOverlayTrack : OverlayTrack
    {
        public string Path { get; set; }
        public Margin Margin { get; set; } = new Margin(0, 0, 0, 0);
        public Align Align { get; set; } = Align.Right;
        public Size Size { get; set; } = new Size(64, 64);
        public double Alpha { get; set; } = 1.0;

        public override List<string> GetInputs(CommandBuilderContext context)
        {
            return new List<string> { "-loop", "1", "-i", Path };
        }

        public override object GetFilter(CommandBuilderContext context)
        {

            var size = Size ?? context?.Settings?.Resolution;
            var location = Alignment.GetLocation(Align, context?.Settings?.Resolution, size, Margin);
            var resultLabel = $"[ov{(context?.InputIndex ?? 0)}]";
            var intermediateLabel = $"[ov{(context?.InputIndex ?? 0)}_i]";
            var filters = new List<string>();
            if (Alpha < 1.0)
                filters.Add($"format=rgba,colorchannelmixer=aa={Alpha:0.##}");
            if (size != null)
                filters.Add($"scale={size.Width}:{size.Height}");
            if (FilterInfo != null && context != null)
            {
                FilterInfo.InputIndex = context.InputIndex;
                FilterInfo.InputLabel = $"[{context.InputIndex}:v]";
                FilterInfo.OutputLabel = resultLabel;
                FilterInfo.Filters = new List<string>
                {
                    $"{string.Join(",", filters)}{intermediateLabel}",
                    $"{{ctx.CurrentLabel}}{intermediateLabel}overlay=x={location.X}:y={location.Y}"
                };
            }
            if (context != null)
            {
                context.CurrentLabel = resultLabel;
                context.OverlaysTracks?.Add(this);
            }
            return new FilterResult("", resultLabel);
        }

        public override string ToString()
        {
            return $"ImageOverlay: {Path} {FilterInfo}";
        }
    }
}
