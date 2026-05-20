namespace AudioBookStudio.Films.Tests.Models;

public class VideoTrackTests
{
    private class TestVideoTrack(double duration) : VideoTrack
    {
        private readonly double _duration = duration;

        public override void BuildFilter(CommandBuilderContext context)
        {
            var size = Size ?? context?.Settings?.Resolution;
            var duration = _duration;
            var filters = new FilterList();
            if (size != null)
                filters.Add($"scale={size.Width}:{size.Height}");
            if (Alpha < 1.0)
                filters.Add("format=rgba,colorchannelmixer=aa=" + Alpha.ToString("0.##"));
            else if (context != null)
                filters.Add($"format={context.Settings.PixelFormat}");
            if (FadeIn > 0)
                filters.Add($"fade=t=in:st=0:d={FadeIn}");
            if (FadeOut > 0 && duration > 0)
                filters.Add($"fade=t=out:st={duration - FadeOut}:d={FadeOut}");
            Filter.InputIndex = context?.InputIndex ?? 0;
            Filter.InputLabel = $"[{(context?.InputIndex ?? 0)}:v]";
            Filter.OutputLabel = $"[v{(context?.InputIndex ?? 0)}]";
            Filter.Filters = filters;
            context?.ConnectTracks?.Add(this);
        }
    }

    [Fact]
    public void BuildInputs_ReturnsLoopArgs_WhenDurationIsZero()
    {
        var track = new VideoTrack { Path = "video.mp4", Duration = 0 };
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Equal(["-stream_loop", "-1", "-i", "video.mp4"], result);
    }

    [Fact]
    public void BuildFilter_SetsFiltersWithFadeInAndFadeOut()
    {
        var track = new TestVideoTrack(20.0)
        {
            Path = "video.mp4",
            Size = new Size(640, 480),
            Alpha = 0.8,
            FadeIn = 2.0,
            FadeOut = 3.0
        };
        var context = new CommandBuilderContext(new FilmSettings { PixelFormat = "yuv420p" }) { InputIndex = 1 };
        track.BuildFilter(context);
        var filters = track.Filter.Filters;
        Assert.Contains("scale=640:480", filters);
        Assert.Contains("format=rgba,colorchannelmixer=aa=0.8", filters);
        Assert.Contains("fade=t=in:st=0:d=2", filters);
        Assert.Contains("fade=t=out:st=17:d=3", filters); // 20 - 3 = 17
        Assert.Equal("[1:v]", track.Filter.InputLabel);
        Assert.Equal("[v1]", track.Filter.OutputLabel);
        Assert.Contains(track, context.ConnectTracks);
    }

    [Fact]
    public void BuildFilter_SetsFormatIfAlphaIsOne()
    {
        var track = new TestVideoTrack(10.0)
        {
            Path = "video.mp4",
            Size = new Size(320, 240),
            Alpha = 1.0
        };
        var context = new CommandBuilderContext(new FilmSettings { PixelFormat = "yuv444p" }) { InputIndex = 2 };
        track.BuildFilter(context);
        var filters = track.Filter.Filters;
        Assert.Contains("format=yuv444p", filters);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var track = new VideoTrack { Path = "video.mp4" };
        Assert.Contains("VideoTrack: video.mp4", track.ToString());
    }
}
