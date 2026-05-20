namespace AudioBookStudio.Models.Data;
public class RegexModel : Entity
{
    public string? Name { get; set; }
    public string? Regex { get; set; }
    public string? Description { get; set; }
    public string? Replace { get; set; }
    public string? Icon { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
