namespace Matrix.Audio.Models;
public class AlbumMeta
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Artist { get; set; }
    public string? ImageWideSplash { get; set; }
    public string? ImageSquareSplash { get; set; }

    public List<EpisodeMeta> Episodes { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> Categories { get; set; } = [];

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
