using System.Text.RegularExpressions;

namespace AudioBookStudio.Films.Extensions;
public static class StringExtensions
{
    public static string BreakLines(this string input, int min = 4, int max = 14)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        input = Regex.Replace(input, @"[‘’“”'\""]", "");

        var sentences = Regex.Split(input, @"(?<=[。！？；])")
                             .Select(s => s.Trim())
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .ToList();

        var merged = new List<string>();
        string buffer = "";

        foreach (var sentence in sentences)
        {
            if ((buffer + sentence).Length < min)
            {
                buffer += sentence;
            }
            else
            {
                if (!string.IsNullOrEmpty(buffer))
                {
                    merged.Add(buffer);
                    buffer = "";
                }

                if (sentence.Length < min)
                {
                    buffer = sentence;
                }
                else
                {
                    merged.Add(sentence);
                }
            }
        }
        if (!string.IsNullOrEmpty(buffer))
            merged.Add(buffer);

        var result = new List<string>();
        foreach (var line in merged)
        {
            int start = 0;
            while (start < line.Length)
            {
                int len = Math.Min(max, line.Length - start);

                if (start + len < line.Length &&
                    IsPunctuation(line[start + len]))
                {
                    len--;
                }

                string chunk = line.Substring(start, len);

                if (chunk.Length > 0 && IsPunctuation(chunk[0]) && result.Count > 0)
                {
                    result[result.Count - 1] += chunk;
                }
                else
                {
                    result.Add(chunk);
                }

                start += len;
            }
        }

        return string.Join("\n", result).Trim();
    }

    private static bool IsPunctuation(char c)
    {
        return "，。！？；,".Contains(c);
    }

    public static async Task<bool> WaitForFileCreated(this string filePath, TimeSpan? timeout = null, TimeSpan? pollInterval = null)
    {
        var start = DateTime.UtcNow;
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(500);

        while ((DateTime.UtcNow - start) < effectiveTimeout)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
            await Task.Delay(interval);
        }

        return false;
    }
}
