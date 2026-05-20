

namespace AudioBookStudio.Models.Data;

public class Chapter
{
    public string? Id { get; set; }

    public string? Title { get; set; }

    public string? FileName { get; set; }

    public string? Content { get; set; }

    public bool IsSelected { get; set; }

    public IList<ChapterChunk> Chunks { get; set; } = [];

    public IList<IResource> Outputs { get; set; } = [];
}
