namespace Matrix.Audio.Models;
public class UserMessage : Entity
{
    public required string Subject { get; set; }
    public required string Content { get; set; }
    public required string UserId { get; set; }
    public DateTime DateCreated { get; set; }
}
