using AudioBookStudio.Films.Common;
using AudioBookStudio.Films.Models;
using System.Diagnostics;
using System.Text;

namespace AudioBookStudio.Films.Extensions;
public static class FilmExtensions
{

    public static Film AddImage(this Film film, string path,
        double start = 0.0, double duration = 4.5,
        string? transition = null, double transitionDuration = 0.5, double alpha = 1.0,
        double fadeInStart = 0.0, double fadeInDuration = 0.0)
    {
        var imageTrack = new ImageTrack
        {
            Path = path,
            Start = start,
            Duration = duration,
            Transition = transition,
            TransitionDuration = transitionDuration,
            Alpha = alpha,
            FadeInStart = fadeInStart,
            FadeInDuration = fadeInDuration
        };
        film.AddTrack(imageTrack);
        return film;
    }

    public static Film AddImagesFromFolder(this Film film, string folder, double imageDuration = 5.0, string? transition = null, double transitionDuration = 0.5, double alpha = 1.0)
    {
        var imageFiles = folder.EnumerateImageFiles();
        double currentTime = 0.0;

        foreach (var imageFile in imageFiles)
        {
            var imageTrack = new ImageTrack
            {
                Path = imageFile,
                Start = currentTime,
                Duration = imageDuration,
                Transition = transition,
                TransitionDuration = transitionDuration,
                Alpha = alpha
            };
            film.AddTrack(imageTrack);
            currentTime += imageDuration;
        }
        return film;
    }

    public static Film AddImageOverlay(this Film film, string path, Margin? margin = null, Size? size = null, double alpha = 1.0, Align align = Align.Right)
    {
        var overlayImageTrack = new ImageOverlayTrack
        {
            Path = path,
            Size = size,
            Margin = margin ?? new Margin(),
            Alpha = alpha,
            Align = align
        };
        film.AddTrack(overlayImageTrack);
        return film;
    }

    public static Film AddVideoOverlay(this Film film, string path, double start = 0, double? duration = null, Location? location = null, Size? size = null, double alpha = 1.0)
    {
        var overlayVideoTrack = new VideoOverlayTrack
        {
            Path = path,
            Start = start,
            Duration = duration ?? 0,
            Location = location ?? new Location(0, 0),
            Size = size,
            Alpha = alpha
        };
        film.AddTrack(overlayVideoTrack);
        return film;
    }

    public static Film AddTextLayer(this Film film, TextModel text, double start = 0, double duration = 0, Location? location = null, double alpha = 1.0)
    {
        var textTrack = new TextTrack
        {
            Text = text,
            Start = start,
            Duration = duration,
            Location = location ?? new Location(0, 0),
            Alpha = alpha
        };
        film.AddTrack(textTrack);
        return film;
    }

    public static Film AddTextGroup(this Film film,
                                         IList<TextModel> texts,
                                         double start = 0,
                                         double duration = 0,
                                         Margin? margin = null,
                                         int lineSpacing = 10,
                                         double alpha = 1.0,
                                         double fadeInStart = 0.0,
                                         double fadeInDuration = 0.0,
                                         Align align = Align.Center
                                         )
    {
        var textGroupTrack = new TextGroupTrack
        {
            Texts = texts,
            Start = start,
            Duration = duration,
            Margin = margin ?? Margin.Empty,
            LineSpacing = lineSpacing,
            Align = align,
            Alpha = alpha,
            FadeInDuration = fadeInDuration,
            FadeInStart = fadeInStart
        };
        film.AddTrack(textGroupTrack);
        return film;
    }

    public static Film AddAudioTrack(this Film film, string path, double start = 0.0, double duration = 0.0,
                                            bool appendDuration = true)
    {
        var audioTrack = new AudioTrack
        {
            Path = path,
            Start = start,
            Duration = duration,
            AppendDuration = appendDuration
        };
        film.AddTrack(audioTrack);
        return film;
    }

    public static Film AddRepeatVideoTrack(this Film film, string videoPath, string audioPath, double start = 0.0, double duration = 0.0)
    {
        var repeatVideoTrack = new RepeatVideoTrack
        {
            AudioPath = audioPath,
            VideoPath = videoPath,
            Start = start,
            Duration = duration
        };
        film.AddTrack(repeatVideoTrack);
        return film;
    }

    public static Film AddImageAudioTrack(this Film film, string imagePath, string audioPath,
                                                double start = 0.0, double duration = 0.0,
                                               string? transition = null, double transitionDuration = 0.0,
                                               double fadeInStart = 0, double fadeInDuration = 0)
    {
        var imageAudioTrack = new ImageAudioTrack
        {
            ImagePath = imagePath,
            AudioPath = audioPath,
            Start = start,
            Duration = duration,
            Transition = transition,
            TransitionDuration = transitionDuration,
            FadeInStart = fadeInStart,
            FadeInDuration = fadeInDuration
        };
        film.AddTrack(imageAudioTrack);
        return film;
    }

    public static Film AddVideoTrack(this Film film, string path, double start = 0.0, double duration = 0.0, double fadeIn = 0.0,
                                            double fadeOut = 0.0, string? transition = null, double transitionDuration = 0.0)
    {
        var videoTrack = new VideoTrack
        {
            Path = path,
            Start = start,
            Duration = duration,
            FadeIn = fadeIn,
            FadeOut = fadeOut,
            Transition = transition,
            TransitionDuration = transitionDuration
        };
        film.AddTrack(videoTrack);
        return film;
    }

    public static Film AddLogo(this Film film, LogoModel logo)
    {
        film
        .AddImageOverlay(
            path: logo.ImagePath,
            size: logo.ImageSize,
            margin: logo.ImageMargin,
            align: logo.ImageAlign,
            alpha: logo.Alpha
        )
        .AddTextGroup(
            texts: logo.GetTextModels(),
            margin: logo.TextMargin,
            lineSpacing: logo.LineSpacing,
            align: logo.TextAlign
        );
        return film;
    }

    public static Film AddTextBlock(this Film film, TextBlock block)
    {
        film.AddTextGroup(
            texts: block.ToTextModel(),
            margin: block.TextMargin,
            align: Align.Left
        );

        return film;
    }

    public static Film AddTextBlocks(this Film film, IEnumerable<TextBlock> blocks)
    {
        foreach (var block in blocks)
        {
            film.AddTextBlock(block);
        }

        return film;
    }

    public static Film AddTextBlocks(this Film film, TextBlocks block, int lineSpacing = 10)
    {
        foreach (var line in block.Lines)
        {
            film.AddTextGroup(
                line.ToTextModel(),
                block.Start,
                block.Duration,
                margin: line.TextMargin,
                align: line.Align,
                lineSpacing: lineSpacing
                );
        }
        return film;
    }

    public static Film AddImageBlock(this Film film, ImageBlock block)
    {
        film.AddImage(
            block.ImagePath,
            duration: block.Duration,
            fadeInDuration: block.FadeInDuration,
            transition: block.Transition,
            transitionDuration: block.TransitionDuration
            );
        return film;
    }

    public static Film AddImageBlocks(this Film film, ImageBlocks blocks)
    {
        foreach (var block in blocks.Blocks)
        {
            film.AddImageBlock(block);
        }
        return film;
    }

    public static FilterList ExportFfmpegCommand(this Film film)
    {
        var context = new CommandBuilderContext(film.Settings);
        var builder = new CommandBuilder(film);
        var cmd = builder.Build(context);
        return cmd;
    }

    public static async Task Export(this Film film, Action<string>? log = null)
    {
        var context = new CommandBuilderContext(film.Settings);
        var builder = new CommandBuilder(film);
        var cmd = builder.Build(context);

        if (film.Settings.Debug)
        {
            log?.Invoke("FFmpeg Command:\n" + string.Join(" ", cmd));
            return;
        }
        try
        {
            void OutputHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    log?.Invoke($"Output: {e.Data}");
            }

            void ErrorHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    log?.Invoke($"Error: {e.Data}");
            }

            var arguments = string.Join(" ", cmd.Skip(1));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = cmd[0],
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                }
            };
            process.OutputDataReceived += OutputHandler;
            process.ErrorDataReceived += ErrorHandler;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            process.OutputDataReceived -= OutputHandler;
            process.ErrorDataReceived -= ErrorHandler;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Exception running FFmpeg: {ex.Message}");
        }
    }
}
