using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AudioBookStudio.Common.IO;
public partial class VideoInfo(string filePath)
{
    private readonly string _filePath = filePath;
    private bool _isInfoRetrieved = false;
    private int _bitrate;
    private TimeSpan _duration;

    public TimeSpan Duration
    {
        get
        {
            EnsureInfoRetrieved();
            return _duration;
        }
    }

    public int Bitrate
    {
        get
        {
            EnsureInfoRetrieved();
            return _bitrate;
        }
    }

    private void EnsureInfoRetrieved()
    {
        if (_isInfoRetrieved)
            return;

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-i \"{_filePath}\"",
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = startInfo })
        {
            process.Start();
            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // Parse the duration and bitrate from FFmpeg output
            _duration = ParseDuration(output);
            _bitrate = ParseBitrate(output);
        }

        _isInfoRetrieved = true;
    }

    private static TimeSpan ParseDuration(string ffmpegOutput)
    {
        var match = DurationRegex().Match(ffmpegOutput);
        if (match.Success)
        {
            int hours = int.Parse(match.Groups[1].Value);
            int minutes = int.Parse(match.Groups[2].Value);
            int seconds = int.Parse(match.Groups[3].Value);
            int milliseconds = int.Parse(match.Groups[4].Value) * 10;

            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        return TimeSpan.Zero;
    }

    private static int ParseBitrate(string ffmpegOutput)
    {
        var match = BitrateRegex().Match(ffmpegOutput);
        if (match.Success)
        {
            return int.Parse(match.Groups[1].Value);
        }

        return 0;
    }

    [GeneratedRegex(@"bitrate: (\d+) kb/s")]
    private static partial Regex BitrateRegex();

    [GeneratedRegex(@"Duration: (\d{2}):(\d{2}):(\d{2})\.(\d{2})")]
    private static partial Regex DurationRegex();
}
