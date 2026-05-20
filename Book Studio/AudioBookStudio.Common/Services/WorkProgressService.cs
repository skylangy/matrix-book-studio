using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Services;

public sealed class WorkProgressService : IWorkProgressService
{
    private readonly Dictionary<string, Action<WorkProgress>> _handlers = [];

    public void AddHandler(string name, Action<WorkProgress> handler)
    {
        if (handler != null && !_handlers.ContainsKey(name))
            _handlers.Add(name, handler);
    }

    public void UpdateProgress(WorkProgress progress)
    {
        foreach (var action in _handlers.Values)
        {
            action(progress);
        }
    }
}