namespace AudioBookStudio.Models.Data;

public class Author : Entity
{
    public string? Name { get; set; }
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public List<string> ImageIds { get; set; } = [];
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
