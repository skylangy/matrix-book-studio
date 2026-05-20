namespace AudioBookStudio.Films.Models;
public static class Alignment
{
    public static Location GetLocation(Align align, Size canvasSize, Size componentSize, Margin margin, int maxWidth = 0)
    {
        var result = new Location(margin.Left, margin.Top);
        if (align == Align.Left)
        {
            result = new Location(margin.Left, margin.Top);
        }
        else if (align == Align.Center)
        {
            result = new Location((int)(margin.Left + (maxWidth - componentSize.Width) / 2), margin.Top);
        }
        else if (align == Align.Right)
        {
            result = new Location(margin.Left + componentSize.Width, margin.Top);
        }
        else if (align == Align.TopRight)
        {
            result = new Location(canvasSize.Width - componentSize.Width - margin.Right, margin.Top);
        }
        else if (align == Align.BottomCenter)
        {
            result = new Location((int)(margin.Left + (canvasSize.Width - componentSize.Width) / 2), canvasSize.Height - margin.Bottom - componentSize.Height);
        }
        return result;
    }
}