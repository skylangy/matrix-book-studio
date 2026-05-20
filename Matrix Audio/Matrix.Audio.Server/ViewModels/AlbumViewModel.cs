namespace Matrix.Audio.Server.ViewModels;

public class AlbumViewModel : ViewModel
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string? Categories { get; set; }
    public string? Status { get; set; }
    public string? ImageWideSplash { get; set; }
    public string? ImageSquareSplash { get; set; }
    public string? ArtistName { get; set; }
    public string? ArtistId { get; set; }
    public int Year { get; set; }
    public long DownloadCount { get; set; }
    public long PlayCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public int EpisodeCount { get; set; }
    public int Level { get; set; } = 1000;
    public double DurationInSeconds { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }

    public ArtistViewModel Artist { get; set; } = new ArtistViewModel();
    public List<EpisodeViewModel> Episodes { get; set; } = [];
}
