namespace MatrixBook.Tray.Services;
public interface IBackgroundService
{
    void Start(CancellationToken cancellationToken);
    void Stop();

    bool IsRunning { get; }
}
