using System.Collections.Generic;

namespace FilmCS
{
    public static class FilmBuilder
    {
        public static Film BuildFilmWithIntroOutro(
            string introPath,
            string mainVideoPath,
            string outroPath,
            string audioPath,
            double introDuration = 3.0,
            double outroDuration = 3.0,
            FilmSettings settings = null)
        {
            var film = new Film(settings);
            film.AddImage(introPath, 0.0, introDuration)
                .AddRepeatVideoTrack(mainVideoPath, audioPath, introDuration)
                .AddImage(outroPath, 0.0, outroDuration)
                .AddAudioTrack(audioPath, introDuration);
            return film;
        }

        public static Film BuildFilmWithImagesLogo(
            string imagesFolder,
            string overlayVideoPath,
            string logoImagePath,
            List<TextModel> nameTexts = null,
            List<TextModel> logoTexts = null,
            double imagesDuration = 4.5,
            string imageTransition = "fade",
            double imageTransitionDuration = 0.5,
            double overlayVideoAlpha = 0.2,
            Margin nameTextsMargin = null,
            Align nameTextsAlign = Align.Center,
            Size logoImageSize = null,
            Margin logoImageMargin = null,
            Align logoImageAlign = Align.TopRight,
            double logoImageAlpha = 1.0,
            Margin logoTextMargin = null,
            Align logoTextAlign = Align.TopRight,
            FilmSettings settings = null)
        {
            var film = new Film(settings);
            film.AddImagesFromFolder(imagesFolder, imagesDuration, imageTransition, imageTransitionDuration)
                .AddVideoOverlay(overlayVideoPath, alpha: overlayVideoAlpha)
                .AddTextGroup(nameTexts, margin: nameTextsMargin, align: nameTextsAlign, lineSpacing: 15)
                .AddImageOverlay(logoImagePath, size: logoImageSize, margin: logoImageMargin, align: logoImageAlign, alpha: logoImageAlpha)
                .AddTextGroup(logoTexts, margin: logoTextMargin, lineSpacing: 15, align: logoTextAlign);
            return film;
        }
    }
}
