namespace MatrixBook.Tray.Models;
public class ServerInfo
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public bool Enabled { get; set; } = true;
}
