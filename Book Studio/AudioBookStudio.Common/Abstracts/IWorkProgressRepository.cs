using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface IWorkProgressRepository
{
    Task<WorkProgress> AddAsync(WorkProgress progress);

    Task<IEnumerable<WorkProgress>> GetByCategoryAsync(string category);

    Task<IEnumerable<WorkProgress>> GetWorkingItemsAsync();
}
