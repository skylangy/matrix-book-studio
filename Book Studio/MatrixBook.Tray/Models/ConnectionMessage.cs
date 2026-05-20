namespace MatrixBook.Tray.Models;
public class ConnectionMessage(string serverName, bool isConnected)
{
    public string ServerName { get; set; } = serverName;

    public bool IsConnected { get; private set; } = isConnected;
}
