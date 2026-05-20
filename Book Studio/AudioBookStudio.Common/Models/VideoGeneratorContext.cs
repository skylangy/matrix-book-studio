namespace AudioBookStudio.Common.Models;

public class VideoGeneratorContext
{
    public string AudioFile { get; set; }
    public string VideoFile { get; set; }
    public string MetadataTitle { get; set; }
    public string Book { get; set; }
    public string Chapter { get; set; }

    //public IList<string> Images { get; set; } = new List<string>();
    public Dictionary<string, NamedImageFile> Images { get; set; } = [];
}