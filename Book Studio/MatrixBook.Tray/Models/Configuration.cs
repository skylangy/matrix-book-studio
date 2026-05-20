namespace MatrixBook.Tray.Models;
public class Configuration
{
    public List<ServerInfo> Servers { get; set; } = [];
    public Dictionary<string, string> PathMapping { get; set; } = [];
}
