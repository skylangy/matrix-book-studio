using MatrixBook.Tray.Models;

namespace MatrixBook.Tray.Services;
public interface IMessageClient
{
    Task StartAsync();
    Task SendMessageAsync(string message);
    void AddExecuteCommandHandler(Action<CommandModel> handler);
    void AddSendMessageHandler(Action<string> handler);
}
