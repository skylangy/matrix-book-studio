namespace Matrix.Audio.Models;
public class EpisodeMeta
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public double Duration { get; set; }
    public long FileLength { get; set; }
    public int Order { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
