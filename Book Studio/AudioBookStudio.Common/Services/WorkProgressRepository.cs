using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Raven.Client.Documents;

namespace AudioBookStudio.Common.Services;
public class WorkProgressRepository(
    IDocumentStore documentStore) : RavenRepository<WorkProgress>(documentStore), IWorkProgressRepository
{
    public async Task<IEnumerable<WorkProgress>> GetByCategoryAsync(string category)
    {
        using var session = _documentStore.OpenAsyncSession();
        var result = await session.Query<WorkProgress>()
                                    .Where(x => x.Category == category)
                                    .OrderByDescending(x => x.Timestamp)
                                    .ToListAsync();

        return result;
    }

    public async Task<IEnumerable<WorkProgress>> GetWorkingItemsAsync()
    {
        using var session = _documentStore.OpenAsyncSession();
        var works = await session.Query<WorkProgress>()
                             .Where(x => x.Status == BookStatus.InProgress)
                             .ToListAsync();

        // Group in memory
        var groupedWorks = works
            .GroupBy(x => new { x.Category, x.Name })
            .Select(g => g.OrderByDescending(x => x.Timestamp).First())
            .OrderByDescending(x => x.Timestamp);

        return groupedWorks;
    }

}
