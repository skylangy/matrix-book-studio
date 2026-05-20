using System.Collections.Generic;

namespace FilmCS
{
    public class VideoOverlayTrack : OverlayTrack
    {
        public string Path { get; set; }
        public Location Location { get; set; } = new Location(0, 0);
        public Size Size { get; set; }
        public double Alpha { get; set; } = 1.0;
        public double Duration { get; set; } = 0.0;

        public override List<string> GetInputs(object context)
        {
            var cmd = new List<string> { "-stream_loop", "-1", "-i", Path };
            if (Duration > 0)
            {
                cmd.Insert(0, Duration.ToString());
                cmd.Insert(0, "-t");
            }
            return cmd;
        }

        public override object GetFilter(CommandBuilderContext context)
        {
            var location = Location;
            var size = Size ?? context?.Settings?.Resolution;
            var resultLabel = $"[ov{(context?.InputIndex ?? 0)}]";
            var intermediateLabel = $"[ov{(context?.InputIndex ?? 0)}_i]";
            var filters = new List<string> { "format=rgba" };
            if (Alpha < 1.0)
                filters.Add($"colorchannelmixer=aa={Alpha:0.##}");
            if (size != null)
                filters.Add($"scale={size.Width}:{size.Height}");
            if (FilterInfo != null && context != null)
            {
                FilterInfo.InputIndex = context.InputIndex;
                FilterInfo.OutputLabel = $"[ov{context.InputIndex}_v]";
                FilterInfo.InputLabel = $"[{context.InputIndex}:v]";
                FilterInfo.Filters = new List<string>
                {
                    $"{string.Join(",", filters)}{intermediateLabel}",
                    $"{{ctx.CurrentLabel}}{intermediateLabel}overlay={location.X}:{location.Y}"
                };
            }
            if (context != null)
            {
                context.CurrentLabel = resultLabel;
                context.OverlaysTracks?.Add(this);
            }
            return new FilterResult("", FilterInfo?.OutputLabel);
        }

        public override string ToString()
        {
            return $"VideoOverlayTrack: {Path} {FilterInfo}";
        }
    }
}
