namespace AudioBookStudio.Models.Data;
public class MediaResource : Entity
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }

    public MediaMetadata? Metadata { get; set; }
}
