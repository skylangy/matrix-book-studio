namespace AudioBookStudio.Films.Models;
public class LogoModel
{
    public required string ImagePath { get; set; }
    public required string Text { get; set; }
    public string FontName { get; set; } = "方正启体繁体";
    public double Alpha { get; set; } = 1.0;
    public int FontSize { get; set; } = 36;
    public int LineSpacing { get; set; } = 15;
    public bool ShowShadow { get; set; } = true;
    public Size ImageSize { get; set; } = new Size(96, 96);
    public Margin ImageMargin { get; set; } = new Margin(0, 60, 70, 0);
    public Margin TextMargin { get; set; } = new Margin(10, 40, 170, 0);
    public Align ImageAlign { get; set; } = Align.TopRight;
    public Align TextAlign { get; set; } = Align.TopRight;

    public List<TextModel> GetTextModels()
    {
        return [
            new TextModel(Text)
            {
                FontName = FontName,
                FontSize = FontSize,
                ShowShadow = ShowShadow,
                Alpha = Alpha
            }
        ];
    }
}

