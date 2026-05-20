namespace Matrix.Audio.Server.ViewModels;

public class EpisodeViewModel : ViewModel
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public double Duration { get; set; }

    public long FileLength { get; set; }

    public int PlayCount { get; set; }
    public int LikeCount { get; set; }
    public int DownloadCount { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }

    public string? AlbumId { get; set; }
    public string? ArtistId { get; set; }
    public string? AlbumTitle { get; set; }
    public string? ArtistName { get; set; }
}
