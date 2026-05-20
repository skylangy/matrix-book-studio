namespace Matrix.Audio.Models;
public class UserSubscription : Entity
{
    public required string UserId { get; set; }
    public required string SubscriptionId { get; set; }
    public required string Name { get; set; }
    public required DateTime DateCreated { get; set; } = DateTime.Now;
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required bool IsActive { get; set; }
    public int PeriodInDays { get; set; }
}
