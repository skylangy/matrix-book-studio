using System.Collections.Generic;

namespace FilmCS
{
    public class ImageTrack : Track
    {
        public string Path { get; set; }
        public Location Position { get; set; } = new Location(0, 0);
        public Size Size { get; set; }
        public string Transition { get; set; }
        public double TransitionDuration { get; set; } = 0.0;
        public double Alpha { get; set; } = 1.0;

        public override List<string> GetInputs(CommandBuilderContext context)
        {
            // Assume context is CommandBuilderContext with Settings and Duration
            double imageDuration = Duration + (Transition != null ? TransitionDuration : 0);
            if (context != null)
                context.Settings.Duration += imageDuration;
            return new List<string> { "-loop", "1", "-t", imageDuration.ToString(), "-i", Path };
        }

        public override object GetFilter(CommandBuilderContext context)
        {
            var size = Size ?? context?.Settings?.Resolution;
            var filters = new List<string>();

            if (size != null)
                filters.Add($"scale={size.Width}:{size.Height}");

            if (Alpha < 1.0)
                filters.Add("format=rgba,colorchannelmixer=aa=" + Alpha.ToString("0.##"));
            else if (context != null)
                filters.Add($"format={context.Settings.PixelFormat}");

            if (FilterInfo != null && context != null)
            {
                FilterInfo.InputIndex = context.InputIndex;
                FilterInfo.InputLabel = $"[{context.InputIndex}:v]";
                FilterInfo.OutputLabel = $"[v{context.InputIndex}]";
                FilterInfo.Filters = filters;
            }
            if (context != null)
            {
                context.CurrentLabel = $"[v{context.InputIndex}]";
                context.ConnectTracks?.Add(this);
            }
            return new FilterResult("", FilterInfo?.OutputLabel);
        }

        public override string ToString()
        {
            return $"ImageTrack: {Path}";
        }
    }
}
