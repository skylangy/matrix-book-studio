namespace AudioBookStudio.Models.Data;
public class MediaResourceGroup
{
    public string Name { get; set; } = string.Empty;

    public List<MediaResource> Resources { get; set; } = [];

    public List<MediaResourceGroup> SubGroups { get; set; } = [];
}
