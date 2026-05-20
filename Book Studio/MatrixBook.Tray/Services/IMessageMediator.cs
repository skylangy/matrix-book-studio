namespace MatrixBook.Tray.Services;
public interface IMessageMediator
{
    void RegisterHandler<T>(Action<T> handler);
    void UnregisterHandler<T>(Action<T> handler);
    void Send<T>(T message);
}

