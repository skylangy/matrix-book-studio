#nullable enable

using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;

public interface IBackgroundWorkerQueue
{
    void Enqueue(WorkItemGroup workItemGroup);

    Task<WorkItemGroup?> DequeueAsync(CancellationToken cancellationToken);
}
