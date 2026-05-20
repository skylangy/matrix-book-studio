namespace Matrix.Audio.Models;
public class Artist : Entity
{
    public string? Name { get; set; }
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}