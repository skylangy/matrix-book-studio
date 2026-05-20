

namespace AudioBookStudio.Models.Data;

public class FileResource : Entity, IResource
{
    public string? Type { get; set; }

    public string? Name { get; set; }

    public string? Url { get; set; }
}
