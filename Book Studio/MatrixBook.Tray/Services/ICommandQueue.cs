using MatrixBook.Tray.Models;

namespace MatrixBook.Tray;
public interface ICommandQueue
{
    void TryEnqueue(CommandModel command);

    CommandModel? TryDequeue();

    public bool IsEmpty { get; }
}
