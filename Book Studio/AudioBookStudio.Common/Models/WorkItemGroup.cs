using System.Collections.Concurrent;

namespace AudioBookStudio.Common.Models;
public class WorkItemGroup(string name, IEnumerable<WorkItem> workItems)
{
    private readonly ConcurrentQueue<WorkItem> _itemsQueue = new(workItems);

    public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

    public int Count => _itemsQueue.Count;

    public void Enqueue(WorkItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        _itemsQueue.Enqueue(item);
    }

    public bool TryDequeue(out WorkItem item)
    {
        return _itemsQueue.TryDequeue(out item);
    }
}
