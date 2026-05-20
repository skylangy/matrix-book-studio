using AudioBookStudio.Films.Extensions;
namespace AudioBookStudio.Films.Tests.Models;

public class FilmTests
{
    [Fact]
    public void AddTrack_AddsTrackToList()
    {
        var film = new Film(new FilmSettings());
        var track = new ImageTrack { Path = "img.png" };
        film.AddTrack(track);
        Assert.Contains(track, film.Tracks);
    }

    [Fact]
    public void AddImage_AddsImageTrack()
    {
        var film = new Film(new FilmSettings());
        film.AddImage("img.png", 1.0, 2.0, "fade", 0.5, 0.8);
        Assert.Single(film.Tracks);
        var track = Assert.IsType<ImageTrack>(film.Tracks[0]);
        Assert.Equal("img.png", track.Path);
        Assert.Equal(1.0, track.Start);
        Assert.Equal(2.0, track.Duration);
        Assert.Equal("fade", track.Transition);
        Assert.Equal(0.5, track.TransitionDuration);
        Assert.Equal(0.8, track.Alpha);
    }

    [Fact]
    public void AddImageOverlay_AddsImageOverlayTrack()
    {
        var film = new Film(new FilmSettings());
        var margin = new Margin(1, 2, 3, 4);
        var size = new Size(10, 20);
        film.AddImageOverlay("overlay.png", margin, size, 0.7, Align.Left);
        var track = Assert.IsType<ImageOverlayTrack>(film.Tracks[0]);
        Assert.Equal("overlay.png", track.Path);
        Assert.Equal(margin, track.Margin);
        Assert.Equal(size, track.Size);
        Assert.Equal(0.7, track.Alpha);
        Assert.Equal(Align.Left, track.Align);
    }

    [Fact]
    public void AddVideoOverlay_AddsVideoOverlayTrack()
    {
        var film = new Film(new FilmSettings());
        var loc = new Location(5, 10);
        var size = new Size(100, 200);
        film.AddVideoOverlay("video.mp4", start: 2.0, duration: 3.0, location: loc, size: size, 1.0);
        var track = Assert.IsType<VideoOverlayTrack>(film.Tracks[0]);
        Assert.Equal("video.mp4", track.Path);
        Assert.Equal(2.0, track.Start);
        Assert.Equal(3.0, track.Duration);
        Assert.Equal(loc, track.Location);
        Assert.Equal(size, track.Size);
        Assert.Equal(0.5, track.Alpha);
    }

    [Fact]
    public void AddTextLayer_AddsTextTrack()
    {
        var film = new Film(new FilmSettings());
        var text = new TextModel("Hello");
        var loc = new Location(1, 2);
        film.AddTextLayer(text, start: 3.0, duration: 4.0, location: loc);
        var track = Assert.IsType<TextTrack>(film.Tracks[0]);
        Assert.Equal(text, track.Text);
        Assert.Equal(3.0, track.Start);
        Assert.Equal(4.0, track.Duration);
        Assert.Equal(loc, track.Location);
    }

    [Fact]
    public void AddTextGroup_AddsTextGroupTrack()
    {
        var film = new Film(new FilmSettings());
        var texts = new List<TextModel> { new("A"), new("B") };
        var margin = new Margin(1, 2, 3, 4);
        film.AddTextGroup(texts, 1.0, 2.0, margin, 5, 0.8);
        var track = Assert.IsType<TextGroupTrack>(film.Tracks[0]);
        Assert.Equal(texts, track.Texts);
        Assert.Equal(1.0, track.Start);
        Assert.Equal(2.0, track.Duration);
        Assert.Equal(margin, track.Margin);
        Assert.Equal(5, track.LineSpacing);
        Assert.Equal(0.8, track.Alpha);
        Assert.Equal(Align.Center, track.Align);
    }

    [Fact]
    public void AddAudioTrack_AddsAudioTrack()
    {
        var film = new Film(new FilmSettings());
        film.AddAudioTrack("audio.mp3", 1.0, 2.0);
        var track = Assert.IsType<AudioTrack>(film.Tracks[0]);
        Assert.Equal("audio.mp3", track.Path);
        Assert.Equal(1.0, track.Start);
        Assert.Equal(2.0, track.Duration);
    }

    [Fact]
    public void AddRepeatVideoTrack_AddsRepeatVideoTrack()
    {
        var film = new Film(new FilmSettings());
        film.AddRepeatVideoTrack("video.mp4", "audio.mp3", 1.0, 2.0);
        var track = Assert.IsType<RepeatVideoTrack>(film.Tracks[0]);
        Assert.Equal("video.mp4", track.VideoPath);
        Assert.Equal("audio.mp3", track.AudioPath);
        Assert.Equal(1.0, track.Start);
        Assert.Equal(2.0, track.Duration);
    }

    [Fact]
    public void AddVideoTrack_AddsVideoTrack()
    {
        var film = new Film(new FilmSettings());
        film.AddVideoTrack("video.mp4", 1.0, 2.0, 0.5, 0.6, "fade", 0.7);
        var track = Assert.IsType<VideoTrack>(film.Tracks[0]);
        Assert.Equal("video.mp4", track.Path);
        Assert.Equal(1.0, track.Start);
        Assert.Equal(2.0, track.Duration);
        Assert.Equal(0.5, track.FadeIn);
        Assert.Equal(0.6, track.FadeOut);
        Assert.Equal("fade", track.Transition);
        Assert.Equal(0.7, track.TransitionDuration);
    }

    [Fact]
    public void SetDebug_UpdatesSettingsDebug()
    {
        var film = new Film(new FilmSettings());
        film.SetDebug(true);
        Assert.True(film.Settings.Debug);
        film.SetDebug(false);
        Assert.False(film.Settings.Debug);
    }
}
