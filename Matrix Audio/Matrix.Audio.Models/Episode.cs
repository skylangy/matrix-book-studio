namespace Matrix.Audio.Models;
public class Episode : Entity
{
    public required string AlbumId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public double Duration { get; set; }
    public long FileLength { get; set; }

    public int Order { get; set; }
    public int PlayCount { get; set; }
    public int LikeCount { get; set; }
    public int DownloadCount { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
