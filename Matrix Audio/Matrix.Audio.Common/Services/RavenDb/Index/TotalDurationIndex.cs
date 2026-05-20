using Matrix.Audio.Models;
using Raven.Client.Documents.Indexes;

namespace Matrix.Audio.Common.Services.RavenDb.Index;
public class TotalDurationIndex : AbstractIndexCreationTask<Episode, TotalDurationIndex.Result>
{
    public class Result
    {
        public double TotalDuration { get; set; }
    }

    public TotalDurationIndex()
    {
        Map = episodes => from episode in episodes
                          select new
                          {
                              TotalDuration = episode.Duration
                          };

        Reduce = results => from result in results
                            group result by 1 into g
                            select new
                            {
                                TotalDuration = g.Sum(x => x.TotalDuration)
                            };
    }
}

