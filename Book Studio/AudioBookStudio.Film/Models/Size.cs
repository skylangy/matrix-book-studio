namespace AudioBookStudio.Films.Models;
public class Size(int width, int height)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;

    public static Size Empty => new(0, 0);

    public bool IsEmpty => Width == 0 && Height == 0;

    public override string ToString()
    {
        return $"Size(Width={Width}, Height={Height})";
    }
}
