namespace Matrix.Audio.Models;
public class PlanFeatures
{
    public static IEnumerable<PlanFeature> InitForSilver(string planId)
    {
        var features = new List<PlanFeature>
        {
            new PlanFeature
            {
                Id = "play-standard-episodes",
                PlanId = planId,
                Name = "PlayStandardEpisodes",
                Description = "Play all episodes for a standard plan",
                IsEnabled = true
            },
            new PlanFeature
            {
                Id = "add-to-playlist",
                PlanId = planId,
                Name = "AddToPlaylist",
                Description = "Add an album to playlist ",
                IsEnabled = false
            }
        };

        return features;
    }

    public static IEnumerable<PlanFeature> InitForGold(string planId)
    {
        var features = new List<PlanFeature>
        {
            new PlanFeature
            {
                Id = "download-episodes",
                PlanId = planId,
                Name = "DownloadEpisode",
                Description = "Download episode",
                IsEnabled = true
            }
        }.Concat(InitForSilver(planId));

        return features;
    }
}
