namespace AudioBookStudio.Films.Tests.Models;

public class VideoOverlayTrackTests
{
    [Fact]
    public void BuildInputs_ReturnsExpectedArgs_WithDuration()
    {
        var track = new VideoOverlayTrack { Path = "video.mp4", Duration = 5.0 };
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Contains("-t", result);
        Assert.Contains("5", result);
        Assert.Contains("video.mp4", result);
    }

    [Fact]
    public void BuildInputs_ReturnsExpectedArgs_WithoutDuration()
    {
        var track = new VideoOverlayTrack { Path = "video.mp4", Duration = 0 };
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Contains("video.mp4", result);
    }

    [Fact]
    public void BuildFilter_SetsFilterInfoAndAddsToOverlaysTracks()
    {
        var track = new VideoOverlayTrack
        {
            Path = "video.mp4",
            Location = new Location(10, 20),
            Size = new Size(320, 240),
            Alpha = 0.7
        };
        var context = new CommandBuilderContext(new FilmSettings { PixelFormat = "yuv420p" }) { InputIndex = 2, CurrentLabel = "[base]" };
        track.BuildFilter(context);
        Assert.Equal("[2:v]", track.Filter.InputLabel);
        Assert.Equal("[ov2_v]", track.Filter.OutputLabel);
        Assert.Contains("format=rgba", string.Join(" ", track.Filter.Filters));
        Assert.Contains("colorchannelmixer=aa=0.7", string.Join(" ", track.Filter.Filters));
        Assert.Contains("scale=320:240", string.Join(" ", track.Filter.Filters));
        Assert.Contains(track, context.OverlaysTracks);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var track = new VideoOverlayTrack { Path = "video.mp4" };
        var str = track.ToString();
        Assert.Contains("VideoOverlayTrack: video.mp4", str);
    }
}
