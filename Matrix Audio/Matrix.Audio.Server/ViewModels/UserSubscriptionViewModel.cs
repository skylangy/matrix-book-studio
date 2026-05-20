namespace Matrix.Audio.Server.ViewModels;

public class UserSubscriptionViewModel
{
    public required string UserId { get; set; }
    public required string SubscriptionId { get; set; }
    public required int PeriodInDays { get; set; }
}
