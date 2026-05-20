using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface ICommandDispatcher
{
    Task<CommandResult?> DispatchAsync(CommandModel command, CancellationToken cancellationToken = default);

    Task Complete(CommandResult result);
}
