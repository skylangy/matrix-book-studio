namespace AudioBookStudio.Models.Data;
public class ChapterMeta
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    /// <summary>
    /// Gets or sets the duration of the chapter in milliseconds.
    /// </summary>
    public double Duration { get; set; }
    public long FileLength { get; set; }
    public int Order { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
