namespace AudioBookStudio.Films.Models;
public class Location(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;

    public override string ToString()
    {
        return $"Location(X={X}, Y={Y})";
    }
}
