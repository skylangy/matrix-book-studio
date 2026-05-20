namespace AudioBookStudio.Films.Tests.Models;

public class ImageTrackTests
{
    [Fact]
    public void BuildInputs_ReturnsExpectedFfmpegArgs()
    {
        var track = new ImageTrack { Path = "test.png", Duration = 5.0 };
        var context = new CommandBuilderContext { Settings = new FilmSettings() };
        var result = track.BuildInputs(context);
        Assert.Equal(["-loop", "1", "-t", "5", "-i", "test.png"], result);
        Assert.Equal(5.0, context.Settings.Duration);
    }

    [Fact]
    public void BuildInputs_WithTransition_AddsTransitionDuration()
    {
        var track = new ImageTrack { Path = "test.png", Duration = 5.0, Transition = "fade", TransitionDuration = 2.0 };
        var context = new CommandBuilderContext { Settings = new FilmSettings() };
        var result = track.BuildInputs(context);
        Assert.Equal(["-loop", "1", "-t", "7", "-i", "test.png"], result);
        Assert.Equal(7.0, context.Settings.Duration);
    }

    [Fact]
    public void BuildFilter_SetsScaleAndAlphaFilters()
    {
        var track = new ImageTrack { Path = "test.png", Size = new Size(100, 200), Alpha = 0.5 };
        var context = new CommandBuilderContext { Settings = new FilmSettings { PixelFormat = "yuv420p" }, InputIndex = 1 };
        track.BuildFilter(context);
        Assert.Contains("scale=100:200", track.Filter.Filters);
        Assert.Contains("format=rgba,colorchannelmixer=aa=0.5", track.Filter.Filters);
        Assert.Equal("[1:v]", track.Filter.InputLabel);
        Assert.Equal("[v1]", track.Filter.OutputLabel);
        Assert.Equal("[v1]", context.CurrentLabel);
        Assert.Contains(track, context.ConnectTracks);
    }

    [Fact]
    public void BuildFilter_UsesContextResolutionIfNoSize()
    {
        var track = new ImageTrack { Path = "test.png" };
        var context = new CommandBuilderContext { Settings = new FilmSettings { Resolution = new Size(320, 240), PixelFormat = "yuv420p" }, InputIndex = 2 };
        track.BuildFilter(context);
        Assert.Contains("scale=320:240", track.Filter.Filters);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var track = new ImageTrack { Path = "image.jpg" };
        Assert.Equal("ImageTrack: image.jpg", track.ToString());
    }
}
