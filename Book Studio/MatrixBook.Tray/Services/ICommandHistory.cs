using MatrixBook.Tray.Models;

namespace MatrixBook.Tray.Services;
public interface ICommandHistory
{
    void Add(CommandHistoryItem item);

    void AddItemHandler(string name, Action<CommandHistoryItem> handler);

    IEnumerable<CommandHistoryItem> All();
}
