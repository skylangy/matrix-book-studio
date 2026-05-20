using Microsoft.Extensions.Logging;
using Moq;

namespace AudioBookStudio.Films.Tests.Models;

public class CommandBuilderTests
{
    private class TestTrack(FilterList inputs, bool hasInput) : Track
    {
        private readonly FilterList _inputs = inputs;

        public override FilterList BuildInputs(CommandBuilderContext context) => _inputs;
        public override void BuildFilter(CommandBuilderContext context) { }
    }

    [Fact]
    public void Build_IncludesFfmpegAndBasicArgs()
    {
        var logger = Mock.Of<ILogger>();
        var film = new Film(new FilmSettings()) { Tracks = [] };
        var builder = new CommandBuilder(film, logger);
        var context = new CommandBuilderContext(new FilmSettings());
        var cmd = builder.Build(context);
        Assert.Contains("ffmpeg", cmd[0]);
        Assert.Contains("-hide_banner", cmd);
        Assert.Contains("-loglevel", cmd);
        Assert.Contains("error", cmd);
    }

    [Fact]
    public void Build_AddsTrackInputsAndFilters()
    {
        var logger = Mock.Of<ILogger>();
        var track = new TestTrack(["-i", "input.mp4"], true);
        var film = new Film(new FilmSettings()) { Tracks = [] };
        var builder = new CommandBuilder(film, logger);
        var context = new CommandBuilderContext(new FilmSettings());
        var cmd = builder.Build(context);
        Assert.Contains("-i", cmd);
        Assert.Contains("input.mp4", cmd);
    }

    [Fact]
    public void Build_AddsFilterComplex_WhenPresent()
    {
        var logger = Mock.Of<ILogger>();
        var mockTrack = new Mock<Track>();
        mockTrack.Setup(t => t.BuildInputs(It.IsAny<CommandBuilderContext>())).Returns([]);
        mockTrack.Setup(t => t.BuildFilter(It.IsAny<CommandBuilderContext>()));
        var film = new Film(new FilmSettings()) { Tracks = [] };
        var builder = new CommandBuilder(film, logger);
        var context = new CommandBuilderContext(new FilmSettings());
        // Simulate overlays/filters
        context.OverlaysTracks.Add(mockTrack.Object);
        mockTrack.SetupGet(t => t.Filter).Returns(new FilterInfo
        {
            InputLabel = "[0:v]",
            OutputLabel = "[v0]",
            Filters = ["scale=320:240"]
        });
        var cmd = builder.Build(context);
        Assert.Contains("-filter_complex", cmd);
        Assert.Contains("scale=320:240", string.Join(" ", cmd));
    }
}
