using AudioBookStudio.Films.Extensions;
using AudioBookStudio.Films.Models;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Films;

public static class FilmBuilder
{
    public static Film BuildFilmWithIntroMainOutro(
        string introPath,
        string mainVideoPath,
        string outroPath,
        string audioPath,
        double introDuration = 3.0,
        double outroDuration = 3.0,
        FilmSettings? settings = null)
    {
        settings ??= new FilmSettings();

        var film = new Film(settings);
        film.AddImage(
            path: introPath,
            start: 0.0,
            duration: introDuration
        ).AddRepeatVideoTrack(
            videoPath: mainVideoPath,
            audioPath: audioPath,
            start: introDuration
        ).AddImage(
            path: outroPath,
            duration: outroDuration
        ).AddAudioTrack(
            path: audioPath,
            start: introDuration
        );
        return film;
    }

    public static Film BuildFilmWithImagesLogo(
        string imagesFolder,
        string overlayVideoPath,
        string logoImagePath,
        List<TextModel> nameTexts,
        List<TextModel> logoTexts,
        double imagesDuration = 4.5,
        string imageTransition = "fade",
        double imageTransitionDuration = 0.5,
        double overlayVideoAlpha = 0.2,
        Margin? nameTextsMargin = null,
        Align nameTextsAlign = Align.Center,
        Size? logoImageSize = null,
        Margin? logoImageMargin = null,
        Align logoImageAlign = Align.TopRight,
        double logoImageAlpha = 1.0,
        Margin? logoTextMargin = null,
        Align logoTextAlign = Align.TopRight,
        FilmSettings? settings = null)
    {
        settings ??= new FilmSettings();
        nameTextsMargin ??= new Margin(left: 50, top: 60, right: 0, bottom: 0);
        logoImageSize ??= new Size(64, 64);
        logoImageMargin ??= new Margin(left: 0, top: 50, right: 50, bottom: 0);
        logoTextMargin ??= new Margin(left: 0, right: 30, top: 120, bottom: 0);

        var film = new Film(settings);
        film
        .AddImagesFromFolder(
            folder: imagesFolder,
            imageDuration: imagesDuration,
            transition: imageTransition,
            transitionDuration: imageTransitionDuration
        ).AddVideoOverlay(
            path: overlayVideoPath,
            alpha: overlayVideoAlpha
        ).AddTextGroup(
            texts: nameTexts,
            margin: nameTextsMargin,
            align: nameTextsAlign,
            lineSpacing: 15
        ).AddImageOverlay(
            path: logoImagePath,
            size: logoImageSize,
            margin: logoImageMargin,
            align: logoImageAlign,
            alpha: logoImageAlpha
        ).AddTextGroup(
            texts: logoTexts,
            margin: logoTextMargin,
            lineSpacing: 15,
            align: logoTextAlign
        );
        return film;
    }


    public static Film BuildYouTubeShort(
        FilmSettings settings,
        string introAudioPath,
        string audioPath,
        ImageBlocks imageBlocks,
        TextBlock nameBlock,
        LogoModel logo,
        TextBlocks snippetBlock,
        double introDuration = 2.0,
        string? metaTitle = null,
        string? metaArtist = null
        )
    {
        var introAudioFile = new MediaFile(introAudioPath);
        var introAudioDuration = introAudioFile.GetDuration();

        var film = new Film(settings);

        film
        .AddImageBlocks(imageBlocks)
        .AddLogo(logo)
        .AddTextBlock(nameBlock)
        .AddTextBlocks(snippetBlock)
        .AddAudioTrack(
            path: introAudioPath,
            appendDuration: false
        )
        .AddAudioTrack(
            path: audioPath,
            start: introDuration - introAudioDuration,
            appendDuration: false
        );

        film.Title = metaTitle;
        film.Artist = metaArtist;

        return film;
    }

    public static async Task BuildBibleShortVideo(BookSnippet snippet, LogoModel logo, string output, ILogger? logger = null)
    {
        var settings = FilmSettings.ForYouTubeShort(output);
        if (snippet.Width.HasValue && snippet.Height.HasValue)
        {
            settings.Resolution = new(snippet.Width.Value, snippet.Height.Value);
        }
        var transitionDuration = 0.5;
        var introDuration = 2.0;

        var audioFile = new MediaFile(snippet.Audio);
        var audioDuration = audioFile.GetDuration();
        var mainDuration = audioDuration + transitionDuration * 2 + 1;

        var imageBlocks = new ImageBlocks() { }
                .AddBlock(new() { ImagePath = snippet.IntroImagePath, Duration = introDuration, FadeInDuration = 1, TransitionDuration = transitionDuration })
                .AddBlock(new() { ImagePath = snippet.BgImagePath, Duration = mainDuration, TransitionDuration = transitionDuration })
                .AddBlock(new() { ImagePath = snippet.OutroImagePath, Duration = introDuration - transitionDuration, TransitionDuration = transitionDuration });

        logger?.LogInformation("Audio duration: {duration}s, main: {main}s,intro duration:{introDuration}s", audioDuration, mainDuration, introDuration);

        var contentFont = snippet.ContentFont ?? FontNames.FZQITS;
        var contentFontSize = 54;
        if (snippet.Content.HasNotSupportCharacters())
        {
            contentFont = FontNames.MicrosoftYaHei;
            contentFontSize = 54;
        }
        if (snippet.ContentFontSize.HasValue)
        {
            contentFontSize = snippet.ContentFontSize.Value;
        }

        var film = BuildYouTubeShort(
            introAudioPath: snippet.IntroAudioPath,
            audioPath: snippet.Audio,
            settings: settings,
            imageBlocks: imageBlocks,
            logo: logo,
            metaTitle: snippet.MetaTitle,
            metaArtist: snippet.MetaArtist,
            nameBlock: new() { Text = snippet.Name, FontSize = 40, FontName = FontNames.Pin8 },
            snippetBlock: new TextBlocks() { Start = introDuration, Duration = mainDuration - transitionDuration }
                .AddLine(new() { Text = snippet.Subtitle, FontSize = 42, FontName = FontNames.AgencyFB, TextMargin = new(50, 0, 200, 0) })
                .AddLine(new() { Text = snippet.Title, FontSize = 32, FontName = FontNames.MicrosoftYaHei, TextMargin = new(50, 0, 260, 0) })
                .AddLine(new() { Text = snippet.Content, FontSize = snippet.ContentFontSize ?? contentFontSize, FontName = contentFont, TextMargin = new(50, 0, 340, 0) })
                .AddLine(new() { Text = snippet.BottomNote, FontSize = 54, FontName = FontNames.BernhardTango, TextMargin = new(0, 0, 0, 80), Align = Align.BottomCenter })
        );

        await film.Export((message) =>
        {
            logger?.LogInformation("{message}", message);
        });
    }

    public static bool HasNotSupportCharacters(this string content)
    {
        string[] words = ["祂", "繸", "痲", "牠"];

        return words.Any(word => content.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}
