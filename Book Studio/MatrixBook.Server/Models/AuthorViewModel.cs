namespace MatrixBook.Server.Models;

public class AuthorViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int Books { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
