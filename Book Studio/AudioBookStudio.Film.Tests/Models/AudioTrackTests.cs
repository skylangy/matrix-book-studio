namespace AudioBookStudio.Films.Tests.Models;

public class AudioTrackTests
{
    private class TestAudioTrack(double duration) : AudioTrack
    {
        private readonly double _duration = duration;

        public override FilterList BuildInputs(CommandBuilderContext context)
        {
            context.Settings.Duration += _duration;
            return ["-i", Path!];
        }
    }

    [Fact]
    public void BuildInputs_ReturnsExpectedArgsAndUpdatesDuration()
    {
        var track = new TestAudioTrack(10.0) { Path = "test.mp3" };
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Equal(new List<string> { "-i", "test.mp3" }, result);
        Assert.Equal(10.0, context.Settings.Duration);
    }

    [Fact]
    public void BuildFilter_SetsCorrectFilterInfoAndAddsToAudioTracks()
    {
        var track = new AudioTrack { Path = "test.mp3", Start = 2.5 };
        var context = new CommandBuilderContext(new FilmSettings()) { InputIndex = 3 };
        track.BuildFilter(context);
        Assert.Equal("[3:a]", track.Filter.InputLabel);
        Assert.Equal("[aud3]", track.Filter.OutputLabel);
        Assert.Contains($"adelay=2500|2500", track.Filter.Filters);
        Assert.Contains(track, context.AudioTracks);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var track = new AudioTrack { Path = "audio.wav" };
        Assert.Equal("AudioTrack: audio.wav", track.ToString());
    }
}
