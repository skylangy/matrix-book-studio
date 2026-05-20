namespace AudioBookStudio.Films.Models;
public class CommandBuilderContext
{
    public FilmSettings Settings { get; set; }
    public string Output { get; set; }
    public int InputIndex { get; set; } = 0;
    public string CurrentLabel { get; set; } = "[base]";
    public string? CurrentAudioLabel { get; set; } = null;
    public List<string> Inputs { get; set; } = [];
    public List<string> Filters { get; set; } = [];
    public List<Track> ConnectTracks { get; set; } = [];
    public List<Track> AudioTracks { get; set; } = [];
    public List<Track> OverlaysTracks { get; set; } = [];
    public Track? PreTrack { get; set; } = null;
    public bool Overwrite { get; set; } = true;

    public CommandBuilderContext(FilmSettings? settings = null)
    {
        Settings = settings ?? new FilmSettings();
        Output = Settings.Output;
    }
}
