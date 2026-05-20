using MatrixBook.Tray.Models;

namespace MatrixBook.Tray.Services;
public class CommandHistory : ICommandHistory
{
    private readonly List<CommandHistoryItem> _items = [];
    private readonly Dictionary<string, Action<CommandHistoryItem>> _itemHandlers = [];

    public void Add(CommandHistoryItem item)
    {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        _items.Add(item);

        foreach (var handler in _itemHandlers.Values)
        {
            handler(item);
        }
    }

    public void AddItemHandler(string name, Action<CommandHistoryItem> handler)
    {
        _itemHandlers[name] = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public IEnumerable<CommandHistoryItem> All()
    {
        return _items.AsReadOnly();
    }
}
