namespace Matrix.Audio.Server.ViewModels;

public class SummaryViewModel
{
    public int AlbumCount { get; set; }
    public int EpisodeCount { get; set; }
    public double DurationInHour { get; set; }
    public int AuthorCount { get; set; }
    public int PostCount { get; set; } = 0;
    public int PromoCount { get; set; } = 0;
    public int UserMessageCount { get; set; } = 0;
    public int UserCount { get; set; } = 0;
    public int FaqCount { get; set; } = 0;
    public int OrderCount { get; set; } = 0;
    public int SubscriptionCount { get; set; } = 0;
    public int OnlineUserCount { get; set; } = 0;

}
