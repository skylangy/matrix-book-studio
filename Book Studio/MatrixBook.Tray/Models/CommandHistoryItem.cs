namespace MatrixBook.Tray.Models;
public class CommandHistoryItem
{
    public CommandHistoryItem()
    {
        CreatedAt = DateTime.Now;
    }

    public DateTime CreatedAt { get; set; }
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;

    public List<string> Details { get; set; } = [];
}
