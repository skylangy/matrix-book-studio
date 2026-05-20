using MatrixBook.Tray.Models;

namespace MatrixBook.Tray.Services;
public interface ICommandRunner
{
    Task RunCommandAsync(CommandModel command, CancellationToken token);
}
