namespace AudioBookStudio.Films.Tests.Models;

public class TextGroupTrackTests
{
    private class TestTextModel : TextModel
    {
        private readonly Size _size;
        public TestTextModel(string text, Size size) : base(text)
        {
            _size = size;
        }
        public override Size CalculateSize() => _size;
    }

    [Fact]
    public void BuildInputs_ReturnsEmptyList()
    {
        var track = new TextGroupTrack();
        var context = new CommandBuilderContext(new FilmSettings());
        var result = track.BuildInputs(context);
        Assert.Empty(result);
    }

    [Fact]
    public void BuildFilter_AddsDrawtextFiltersForEachText()
    {
        var texts = new List<TextModel>
        {
            new TestTextModel("Hello", new Size(50, 20)) { FontSize = 24, FontColor = "red", FontName = "Arial", Alpha = 1.0 },
            new TestTextModel("World", new Size(60, 25)) { FontSize = 30, FontColor = "blue", FontName = "Arial", Alpha = 0.5, ShowShadow = true, ShadowColor = "black", ShadowX = 2, ShadowY = 2 }
        };
        var track = new TextGroupTrack { Texts = texts, Align = Align.Left, Margin = new Margin(5, 0, 10, 0), LineSpacing = 5 };
        var context = new CommandBuilderContext(new FilmSettings { Resolution = new Size(200, 100) }) { InputIndex = 1 };
        track.BuildFilter(context);
        Assert.Equal("[1:v]", track.Filter.InputLabel);
        Assert.StartsWith("[tg1]", track.Filter.OutputLabel);
        Assert.Equal(2, track.Filter.Filters.Count);
        Assert.Contains("drawtext=fontfile=", track.Filter.Filters[0]);
        Assert.Contains("text='Hello'", track.Filter.Filters[0]);
        Assert.Contains("fontsize=24", track.Filter.Filters[0]);
        Assert.Contains("fontcolor=red", track.Filter.Filters[0]);
        Assert.Contains("x=5", track.Filter.Filters[0]);
        Assert.Contains("y=10", track.Filter.Filters[0]);
        Assert.Contains("drawtext=fontfile=", track.Filter.Filters[1]);
        Assert.Contains("text='World'", track.Filter.Filters[1]);
        Assert.Contains("fontsize=30", track.Filter.Filters[1]);
        Assert.Contains("fontcolor=blue@0.5", track.Filter.Filters[1]);
        Assert.Contains("shadowcolor=black", track.Filter.Filters[1]);
        Assert.Contains("shadowx=2", track.Filter.Filters[1]);
        Assert.Contains("shadowy=2", track.Filter.Filters[1]);
        Assert.Contains(track, context.OverlaysTracks);
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var texts = new List<TextModel> { new TestTextModel("A", new Size(10, 10)), new TestTextModel("B", new Size(10, 10)) };
        var track = new TextGroupTrack { Texts = texts };
        Assert.Equal("TextGroup: A, B", track.ToString());
    }
}
