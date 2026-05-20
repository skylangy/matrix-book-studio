using System.Collections.Generic;

namespace FilmCS
{
    public class AudioTrack : Track
    {
        public string Path { get; set; }

        public AudioTrack()
        {
            IsAudio = true;
        }

        public override List<string> GetInputs(object context)
        {
            // Assume context is CommandBuilderContext with Settings and Duration
            var ctx = context as CommandBuilderContext;
            double duration = Duration;
            if (duration == 0 && ctx != null)
            {
                // In real code, get audio duration from file if needed
                // duration = GetAudioDuration(Path);
            }
            if (ctx != null)
                ctx.Settings.Duration += duration;
            return new List<string> { "-i", Path };
        }

        public override object GetFilter(object context)
        {
            var ctx = context as CommandBuilderContext;
            int offset = (int)(Start * 1000);
            if (FilterInfo != null && ctx != null)
            {
                FilterInfo.InputIndex = ctx.InputIndex;
                FilterInfo.OutputLabel = $"[aud{ctx.InputIndex}]";
                FilterInfo.InputLabel = $"[{ctx.InputIndex}:a]";
                FilterInfo.Filters = new List<string> { $"adelay={offset}|{offset}" };
            }
            ctx?.AudioTracks?.Add(this);
            if (Start > 0)
                return new FilterResult($"adelay={offset}|{offset}", FilterInfo.OutputLabel);
            else
                return new FilterResult("", "");
        }

        public override string ToString()
        {
            return $"AudioTrack: {Path} {FilterInfo}";
        }
    }
}
