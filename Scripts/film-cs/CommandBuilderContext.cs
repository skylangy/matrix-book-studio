using System.Collections.Generic;

namespace FilmCS
{
    public class CommandBuilderContext
    {
        public FilmSettings Settings { get; set; }
        public string Output { get; set; }
        public int InputIndex { get; set; } = 0;
        public string CurrentLabel { get; set; } = "[base]";
        public string CurrentAudioLabel { get; set; } = null;
        public List<string> Inputs { get; set; } = new List<string>();
        public List<string> Filters { get; set; } = new List<string>();
        public List<Track> ConnectTracks { get; set; } = new List<Track>();
        public List<Track> AudioTracks { get; set; } = new List<Track>();
        public List<Track> OverlaysTracks { get; set; } = new List<Track>();
        public Track PreTrack { get; set; } = null;
        public bool Overwrite { get; set; } = true;

        public CommandBuilderContext(FilmSettings settings = null)
        {
            Settings = settings ?? new FilmSettings();
            Output = Settings.Output;
        }
    }

    public class FilterResult
    {
        public string Filter { get; set; }
        public string Label { get; set; }
        public FilterResult(string filter, string label)
        {
            Filter = filter;
            Label = label;
        }
    }
}
