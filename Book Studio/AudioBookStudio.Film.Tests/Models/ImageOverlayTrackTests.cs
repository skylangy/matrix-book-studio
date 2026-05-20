namespace AudioBookStudio.Films.Tests.Models;

public class ImageOverlayTrackTests
{
    [Fact]
    public void BuildInputs_ReturnsExpectedArgs()
    {
        var track = new ImageOverlayTrack { Path = "overlay.png" };
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Equal(new List<string> { "-loop", "1", "-i", "overlay.png" }, result);
    }

    [Fact]
    public void BuildFilter_SetsFilterInfoAndAddsToOverlaysTracks()
    {
        var track = new ImageOverlayTrack
        {
            Path = "overlay.png",
            Margin = new Margin(5, 10, 15, 20),
            Align = Align.TopRight,
            Size = new Size(32, 32),
            Alpha = 0.7
        };
        var context = new CommandBuilderContext(new FilmSettings { Resolution = new Size(128, 128) }) { InputIndex = 2, CurrentLabel = "[base]" };
        track.BuildFilter(context);
        Assert.Equal("[2:v]", track.Filter.InputLabel);
        Assert.Equal("[ov2]", track.Filter.OutputLabel);
        Assert.Contains("format=rgba,colorchannelmixer=aa=0.7", string.Join(" ", track.Filter.Filters));
        Assert.Contains("scale=32:32", string.Join(" ", track.Filter.Filters));
        Assert.Contains(track, context.OverlaysTracks);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var track = new ImageOverlayTrack { Path = "overlay.png" };
        var str = track.ToString();
        Assert.Contains("ImageOverlay: overlay.png", str);
    }
}
