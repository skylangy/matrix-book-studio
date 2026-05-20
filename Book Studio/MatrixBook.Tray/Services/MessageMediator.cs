using System.Collections.Concurrent;

namespace MatrixBook.Tray.Services;

public class MessageMediator : IMessageMediator
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    public void RegisterHandler<T>(Action<T> handler)
    {
        var handlers = _handlers.GetOrAdd(typeof(T), _ => []);
        lock (handlers)
        {
            handlers.Add(handler);
        }
    }

    public void UnregisterHandler<T>(Action<T> handler)
    {
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            lock (handlers)
            {
                handlers.Remove(handler);
            }
        }
    }

    public void Send<T>(T message)
    {
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            List<Delegate> snapshot;
            lock (handlers)
            {
                snapshot = new List<Delegate>(handlers);
            }
            foreach (var handler in snapshot)
            {
                if (handler is Action<T> action)
                {
                    action(message);
                }
            }
        }
    }
}
