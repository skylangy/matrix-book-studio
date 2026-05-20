namespace MatrixBook.Server.Models;

public class BookInfo
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Author { get; set; }
    public string? Status { get; set; }
    public long TextCount { get; set; }
    public DateTime? DateUpdated { get; set; }
    public DateTime? DateCreated { get; set; }
}
