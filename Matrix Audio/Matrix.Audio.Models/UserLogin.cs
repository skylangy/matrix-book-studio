namespace Matrix.Audio.Models;
public class UserLogin : Entity
{
    public required string UserId { get; set; }
    public required string UserEmail { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public required DateTime DateCreated { get; set; } = DateTime.UtcNow;
}
