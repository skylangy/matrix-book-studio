using AudioBookStudio.Films.Common;
using AudioBookStudio.Films.Models;
using System.Text;

namespace AudioBookStudio.Common.Extensions;
public static class VideoExtensions
{
    public static LogoModel ToFilmLogo(this VideoLogo logo, string resourceRoot)
    {
        if (logo == null)
        {
            return null;
        }

        var imagePath = Path.Combine(resourceRoot, logo.Image?.Url ?? string.Empty);
        return new LogoModel
        {
            ImagePath = imagePath,
            Text = logo.Text,
            FontSize = logo.FontSize,
            ShowShadow = logo.Shadow,
            FontName = logo.FontFamily
        };
    }

    public static BookSnippet ToFilmSnippet(this VideoMeta videoMeta, string resourceRoot,
        string outputFile)
    {
        if (videoMeta == null)
        {
            return null;
        }
        var snippet = new BookSnippet
        {
            Name = videoMeta.Name,
            Title = videoMeta.Title,
            Subtitle = videoMeta.Subtitle,
            Content = videoMeta.Content,
            BottomNote = videoMeta.BottomNote,
            Audio = Path.Combine(resourceRoot, ResourceTypes.Mp3, $"{videoMeta.GetExportName()}.{ResourceTypes.Mp3}"),
            IntroAudioPath = Path.Combine(resourceRoot, videoMeta.IntroAudio.Url),
            IntroImagePath = Path.Combine(resourceRoot, videoMeta.IntroImage?.Url ?? string.Empty),
            OutroImagePath = Path.Combine(resourceRoot, videoMeta.OutroImage?.Url ?? string.Empty),
            BgImagePath = Path.Combine(resourceRoot, videoMeta.ContentImages.FirstOrDefault()?.Url ?? string.Empty),
            MetaTitle = $"{videoMeta.Title.RemoveColon()}",
            MetaArtist = videoMeta.Tag ?? videoMeta.Name,
            Width = videoMeta.Width,
            Height = videoMeta.Height
        };
        if (videoMeta.ContentFontFamily != null)
        {
            snippet.ContentFont = videoMeta.ContentFontFamily;
        }
        if (videoMeta.ContentFontSize.HasValue)
        {
            snippet.ContentFontSize = videoMeta.ContentFontSize.Value;
        }

        return snippet;
    }

    public static string GetExportName(this VideoMeta videoMeta)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(videoMeta.Tag))
        {
            builder.Append($"{videoMeta.Tag}_");
        }

        if (!string.IsNullOrEmpty(videoMeta.Title))
        {
            builder.Append($"{videoMeta.Title.ToSafeFileName()}_");
        }

        if (!string.IsNullOrEmpty(videoMeta.Name))
        {
            builder.Append($"{videoMeta.Name.ToSafeFileName()}");
        }

        return builder.ToString();
    }

    public static string GetOutputFileName(this VideoMeta videoMeta, string root)
    {
        var fileName = videoMeta.GetExportName();
        return Path.Combine(root, ResourceTypes.Output, $"{videoMeta.Category}", $"{fileName}.{ResourceTypes.Mp4}");
    }

    public static string RemoveColon(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        return input.Replace(":", "-").Replace("/", "-").Replace("\\", "-");
    }
}
