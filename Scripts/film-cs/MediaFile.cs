using System.Diagnostics;

namespace FilmCS
{
    public class MediaFile
    {
        public string Path { get; set; }
        public double Start { get; set; } = 0.0;
        public double Duration { get; set; } = 0.0;

        public MediaFile(string path)
        {
            Path = path;
        }

        public string EscapePath()
        {
            return Path.Replace(" ", "\\ ");
        }

        public double GetDuration()
        {
            // This method uses ffprobe to get the duration of a media file
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{Path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            if (double.TryParse(output.Trim(), out double duration))
                return duration;
            return 0.0;
        }

        public override string ToString()
        {
            return $"MediaFile(path={Path}, start={Start}, duration={Duration})";
        }
    }
}
