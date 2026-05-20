

namespace AudioBookStudio.Films.Tests.Models;

public class AlignmentTests
{
    [Fact]
    public void GetLocation_Left_ReturnsTopLeftWithMargin()
    {
        var loc = Alignment.GetLocation(Align.Left, new Size(100, 100), new Size(20, 20), new Margin(5, 0, 10, 0));
        Assert.Equal(5, loc.X);
        Assert.Equal(10, loc.Y);
    }

    [Fact]
    public void GetLocation_Center_ReturnsCenteredLocation()
    {
        var loc = Alignment.GetLocation(Align.Center, new Size(100, 100), new Size(20, 20), new Margin(0, 0, 0, 0), 100);
        Assert.Equal(40, loc.X); // (0 + (100 - 20) / 2)
        Assert.Equal(10, loc.Y); // (0 + (20 / 2))
    }

    [Fact]
    public void GetLocation_Right_ReturnsRightLocation()
    {
        var loc = Alignment.GetLocation(Align.Right, new Size(100, 100), new Size(20, 20), new Margin(3, 0, 7, 0));
        Assert.Equal(23, loc.X); // 3 + 20
        Assert.Equal(7, loc.Y);
    }

    [Fact]
    public void GetLocation_TopRight_ReturnsTopRightCorner()
    {
        var loc = Alignment.GetLocation(Align.TopRight, new Size(100, 100), new Size(20, 20), new Margin(0, 4, 2, 0));
        Assert.Equal(76, loc.X); // 100 - 20 - 4
        Assert.Equal(2, loc.Y);
    }

    [Fact]
    public void GetLocation_Default_ReturnsMarginLocation()
    {
        var loc = Alignment.GetLocation((Align)999, new Size(100, 100), new Size(20, 20), new Margin(1, 2, 3, 4));
        Assert.Equal(1, loc.X);
        Assert.Equal(3, loc.Y);
    }
}
