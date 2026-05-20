using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface ICommandInvoker
{
    Task InvokeAsync(CommandModel command);
}
