using SkiaSharp;
using System.Drawing;


namespace AudioBookStudio.Films.Models;
public class TextModel(string text)
{
    public string Text { get; set; } = text;
    public int FontSize { get; set; } = 24;
    public string FontColor { get; set; } = "white";
    public string FontName { get; set; } = "Microsoft YaHei";
    public double Alpha { get; set; } = 1.0;
    public bool ShowShadow { get; set; } = false;
    public string ShadowColor { get; set; } = "black";
    public int ShadowX { get; set; } = 2;
    public int ShadowY { get; set; } = 2;

    public virtual Size CalculateSize()
    {
        using var paint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName(FontName),
            TextSize = FontSize,
            IsAntialias = true
        };

        SKRect bounds = new();
        paint.MeasureText(text, ref bounds);
        int width = (int)Math.Ceiling(bounds.Width);
        int height = (int)Math.Ceiling(bounds.Height);
        return new Size(width, height);
    }

    public Size MeasureWithGdi()
    {
        using var bmp = new Bitmap(100, 100);
        using var graphics = Graphics.FromImage(bmp);
        using var font = new Font(FontName, FontSize);


        SizeF size = graphics.MeasureString(Text, font, new PointF(0, 0), StringFormat.GenericTypographic);
        return new Size((int)(size.Width * graphics.DpiX / 96), (int)(size.Height * graphics.DpiY / 96));
    }

    public string EscapeText()
    {
        return Text
        .Replace("\\", "\\\\")     // escape \
        .Replace(":", "\\\\:")     // escape : as \\: for FFmpeg
        .Replace("'", "\\'");      // escape '
    }
}
