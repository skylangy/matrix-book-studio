namespace Matrix.Audio.Models;
public class UserOperation : Entity
{
    public required string UserId { get; set; }
    public required string TargetId { get; set; }
    public required string Operation { get; set; }
    public required string SecondaryId { get; set; }
    public required string Details { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public int Count { get; set; } = 1;
}
