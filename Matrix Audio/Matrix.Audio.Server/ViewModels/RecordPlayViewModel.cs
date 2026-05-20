namespace Matrix.Audio.Server.ViewModels;

public class RecordPlayViewModel
{
    public required string AlbumId { get; set; }
    public required string EpisodeId { get; set; }
    public required string UserId { get; set; }
    public double Position { get; set; }
}
