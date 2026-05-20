namespace AudioBookStudio.Films.Models;

public class Film(FilmSettings settings)
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public List<Track> Tracks { get; set; } = [];
    public FilmSettings Settings { get; set; } = settings ?? new FilmSettings();

    public void AddTrack(Track track)
    {
        Tracks.Add(track);
    }

    public void SetDebug(bool value = true)
    {
        Settings.Debug = value;
    }
}