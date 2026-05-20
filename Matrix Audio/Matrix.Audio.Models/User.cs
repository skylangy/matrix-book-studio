namespace Matrix.Audio.Models;
public class User : Entity
{
    public string? ImageId { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
    public int? Level { get; set; } = 1000;
    public bool IsLocked { get; set; } = false;
    public bool EmailVerified { get; set; } = false;
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
}
