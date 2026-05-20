using System.Text;

namespace AudioBookStudio.Models.Data;
public class SrtLine
{
    public int Index { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public List<string> TextLines { get; set; } = [];

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine(Index.ToString());
        builder.AppendLine($"{FormatTime(Start)} --> {FormatTime(End)}");
        foreach (var line in TextLines)
            builder.AppendLine(line);
        return builder.ToString();
    }

    public void Shift(TimeSpan offset)
    {
        Start += offset;
        End += offset;
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.ToString(@"hh\:mm\:ss\,fff");
    }
}
