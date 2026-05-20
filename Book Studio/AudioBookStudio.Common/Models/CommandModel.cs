namespace AudioBookStudio.Common.Models;
public class CommandModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Command { get; set; }
    public string Arguments { get; set; }
}
