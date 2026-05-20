namespace AudioBookStudio.Films.Models;
public class Margin(int left = 0, int right = 0, int top = 0, int bottom = 0)
{
    public int Left { get; set; } = left;
    public int Right { get; set; } = right;
    public int Top { get; set; } = top;
    public int Bottom { get; set; } = bottom;

    public Margin Copy()
    {
        return new Margin(Left, Right, Top, Bottom);
    }

    public static Margin Empty => new(0, 0, 0, 0);

    public override string ToString()
    {
        return $"Margin(Left: {Left}, Right: {Right}, Top: {Top}, Bottom: {Bottom})";
    }
}
