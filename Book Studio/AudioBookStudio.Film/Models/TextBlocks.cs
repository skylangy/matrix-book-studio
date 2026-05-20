namespace AudioBookStudio.Films.Models;
public class TextBlocks
{
    private readonly List<TextBlock> _lines = [];

    public double Start { get; set; }
    public double Duration { get; set; } = 0.0;

    public TextBlocks()
    {
    }

    public TextBlocks(IEnumerable<TextBlock> lines)
    {
        foreach (var line in lines)
        {
            AddLine(line);
        }
    }

    public IReadOnlyList<TextBlock> Lines => _lines.AsReadOnly();

    public TextBlocks AddLine(TextBlock line)
    {
        ArgumentNullException.ThrowIfNull(line);

        _lines.Add(line);

        return this;
    }

    public List<TextModel> ToTextModel()
    {
        return [.. _lines.Select(line => new TextModel(line.Text)
        {
            FontName = line.FontName,
            FontSize = line.FontSize,
            ShowShadow = line.ShowShadow,
            Alpha = line.Alpha
        })];
    }
}
