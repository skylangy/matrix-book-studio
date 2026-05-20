namespace AudioBookStudio.Films.Models;
public class TextBlock
{
    public required string Text { get; set; }
    public string FontName { get; set; } = "方正启体繁体";
    public double Alpha { get; set; } = 1.0;
    public int FontSize { get; set; } = 40;
    public bool ShowShadow { get; set; } = true;
    public Margin TextMargin { get; set; } = new Margin(50, 0, 100, 0);
    public Align Align { get; set; } = Align.Left;

    public List<TextModel> ToTextModel()
    {
        return [new (Text)
        {
            FontName = FontName,
            FontSize = FontSize,
            ShowShadow = ShowShadow,
            Alpha = Alpha
        }];
    }
}
