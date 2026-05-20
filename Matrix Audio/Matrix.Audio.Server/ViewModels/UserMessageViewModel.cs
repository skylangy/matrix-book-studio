namespace Matrix.Audio.Server.ViewModels;

public class UserMessageViewModel
{
    public required string Subject { get; set; }
    public required string Content { get; set; }
    public required string UserId { get; set; }
    public DateTime DateCreated { get; set; }
}