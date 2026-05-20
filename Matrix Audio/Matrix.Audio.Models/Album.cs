namespace Matrix.Audio.Models;

public class Album : Entity
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Artist { get; set; }
    public string? ArtistId { get; set; }
    public string? ImageWideSplashId { get; set; }
    public string? ImageSquareSplashId { get; set; }

    public List<string> Episodes { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> Categories { get; set; } = [];

    public int Level { get; set; } = 1000;
    public int Year { get; set; }
    public long DownloadCount { get; set; }
    public long PlayCount { get; set; }
    public long LikeCount { get; set; }
    public long CommentCount { get; set; }
    public int Ranking { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
