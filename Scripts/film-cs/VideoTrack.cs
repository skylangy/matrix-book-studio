using System.Collections.Generic;

namespace FilmCS
{
    public class VideoTrack : Track
    {
        public string Path { get; set; }
        public Location Location { get; set; } = new Location(0, 0);
        public Size Size { get; set; }
        public double Alpha { get; set; } = 1.0;
        public double FadeIn { get; set; } = 0.0;
        public double FadeOut { get; set; } = 0.0;
        public string Transition { get; set; }
        public double TransitionDuration { get; set; } = 0.0;

        public override List<string> GetInputs(CommandBuilderContext context)
        {
            // Placeholder: actual implementation would depend on context types
            return new List<string>();
        }

        public override object GetFilter(CommandBuilderContext context)
        {
            // Assume context is CommandBuilderContext with Settings and InputIndex
            // and that FilterResult is a class with (string filter, string label)

            var size = Size ?? context?.Settings?.Resolution;
            var duration = Duration; // In real code, get video duration if Duration == 0
            var filters = new List<string>();

            if (size != null)
                filters.Add($"scale={size.Width}:{size.Height}");

            if (Alpha < 1.0)
                filters.Add("format=rgba,colorchannelmixer=aa=" + Alpha.ToString("0.##"));
            else if (context != null)
                filters.Add($"format={context.Settings.PixelFormat}");

            if (FadeIn > 0)
                filters.Add($"fade=t=in:st=0:d={FadeIn}");
            if (FadeOut > 0 && duration > 0)
                filters.Add($"fade=t=out:st={duration - FadeOut}:d={FadeOut}");

            var resultLabel = $"[v{(context?.InputIndex ?? 0)}]";
            var filterStr = string.Join(",", filters);

            if (FilterInfo != null)
            {
                FilterInfo.InputIndex = context?.InputIndex ?? 0;
                FilterInfo.InputLabel = $"[{(context?.InputIndex ?? 0)}:v]";
                FilterInfo.OutputLabel = resultLabel;
                FilterInfo.Filters = new List<string> { filterStr };
            }
            context?.ConnectTracks?.Add(this);
            Logger?.Info($"Video filter: {filterStr}");
            return new FilterResult("", resultLabel);
        }

        public override string ToString()
        {
            return $"VideoTrack: {Path} {FilterInfo}";
        }
    }
}
