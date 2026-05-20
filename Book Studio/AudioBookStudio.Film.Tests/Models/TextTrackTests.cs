namespace AudioBookStudio.Films.Tests.Models;

public class TextTrackTests
{
    private class TestTextModel(string text) : TextModel(text)
    {
        public override Size CalculateSize() => new(10, 10);
    }

    [Fact]
    public void BuildInputs_ReturnsEmptyList()
    {
        var track = new TextTrack();
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildFilter_AddsDrawtextFilterWithShadowAndAlpha()
    {
        var text = new TestTextModel("Hello")
        {
            FontSize = 24,
            FontColor = "red",
            FontName = "Arial",
            Alpha = 0.5,
            ShowShadow = true,
            ShadowColor = "black",
            ShadowX = 2,
            ShadowY = 3
        };
        var track = new TextTrack { Text = text, Location = new Location(5, 10) };
        var context = new CommandBuilderContext(new FilmSettings()) { InputIndex = 2 };
        track.BuildFilter(context);
        Assert.Equal("[2:v]", track.Filter.InputLabel);
        Assert.Equal("[t2]", track.Filter.OutputLabel);
        var filterStr = string.Join("", track.Filter.Filters);
        Assert.Contains("drawtext=fontfile=", filterStr);
        Assert.Contains("text=\"Hello\"", filterStr);
        Assert.Contains("fontsize=24", filterStr);
        Assert.Contains("x=5", filterStr);
        Assert.Contains("y=10", filterStr);
        Assert.Contains("fontcolor=red@0.5", filterStr);
        Assert.Contains("shadowx=2", filterStr);
        Assert.Contains("shadowy=3", filterStr);
        Assert.Contains("shadowcolor=black", filterStr);
        Assert.Contains(track, context.OverlaysTracks);
    }

    [Fact]
    public void BuildFilter_AddsDrawtextFilterWithoutShadowOrAlpha()
    {
        var text = new TestTextModel("World")
        {
            FontSize = 18,
            FontColor = "blue",
            FontName = "Arial",
            Alpha = 1.0,
            ShowShadow = false
        };
        var track = new TextTrack { Text = text, Location = new Location(0, 0) };
        var context = new CommandBuilderContext(new FilmSettings()) { InputIndex = 1 };
        track.BuildFilter(context);
        var filterStr = string.Join("", track.Filter.Filters);
        Assert.Contains("drawtext=fontfile=", filterStr);
        Assert.Contains("text=\"World\"", filterStr);
        Assert.Contains("fontsize=18", filterStr);
        Assert.Contains("x=0", filterStr);
        Assert.Contains("y=0", filterStr);
        Assert.Contains("fontcolor=blue", filterStr);
        Assert.DoesNotContain("@", filterStr); // No alpha
        Assert.DoesNotContain("shadowx=", filterStr);
        Assert.DoesNotContain("shadowy=", filterStr);
        Assert.DoesNotContain("shadowcolor=", filterStr);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var text = new TestTextModel("Sample");
        var track = new TextTrack { Text = text };
        Assert.Equal("Text: Sample", track.ToString());
    }
}
