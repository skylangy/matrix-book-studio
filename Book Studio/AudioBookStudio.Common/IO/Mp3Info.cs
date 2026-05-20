using System.Diagnostics;
using System.Globalization;

namespace AudioBookStudio.Common.IO;
public class Mp3Info(string filePath)
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

    public Task<Mp3Info> Load()
    {
        EnsureInfoRetrieved();
        return Task.FromResult(this);
    }

    private void EnsureInfoRetrieved()
    {
        if (_isInfoRetrieved)
            return;

        //using var audioFile = new Mp3FileReader(_filePath);
        //_duration = audioFile.TotalTime;
        //_bitrate = audioFile.Mp3WaveFormat.AverageBytesPerSecond * 8 / 1000;
        _duration = GetAudioDuration(_filePath);

        _isInfoRetrieved = true;
    }

    public static TimeSpan GetAudioDuration(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified audio file does not exist.", filePath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (double.TryParse(output.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
        {
            return TimeSpan.FromSeconds(seconds);
        }

        throw new Exception("Failed to parse duration from ffprobe output.");
    }
}
