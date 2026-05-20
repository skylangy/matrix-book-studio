namespace AudioBookStudio.Models.Data;
public class VideoTrack
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<MediaElement> Elements { get; set; }
}