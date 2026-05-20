#nullable enable

using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AudioBookStudio.Common.Services;
public class BackgroundWorkerQueue(ILogger<BackgroundWorkerQueue> logger) : IBackgroundWorkerQueue
{
    private readonly ConcurrentQueue<WorkItemGroup> _items = [];
    private readonly SemaphoreSlim _signal = new(0);
    private readonly ILogger<BackgroundWorkerQueue> _logger = logger;

    public void Enqueue(WorkItemGroup workItemGroup)
    {
        ArgumentNullException.ThrowIfNull(workItemGroup);

        _logger.LogDebug("Enqueuing work item: {Name} ", workItemGroup.Name);

        _items.Enqueue(workItemGroup);
        _signal.Release();
    }

    public async Task<WorkItemGroup?> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _items.TryDequeue(out var workItemGroup);
        return workItemGroup;
    }
}
