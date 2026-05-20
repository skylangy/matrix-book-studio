using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioBookStudio.Models.Extensions;

public static partial class StringExtensions
{
    public static IEnumerable<string> ChunkText(this string input, int chunkSize)
    {
        if (input.IsNullOrEmpty())
        {
            return [];
        }
        if (input.Length < chunkSize)
        {
            return [input];
        }

        var chunks = new List<string>();
        while (input.IsNotNullOrEmpty())
        {
            int length = Math.Min(input.Length, chunkSize);
            int lastParagraphBreakIndex = input.LastIndexOf(Environment.NewLine, length, StringComparison.Ordinal);
            if (lastParagraphBreakIndex < 0)
            {
                chunks.Add(input[..length]);
                input = input[length..].TrimStart();
            }
            else
            {
                chunks.Add(input[..lastParagraphBreakIndex]);
                input = input[lastParagraphBreakIndex..].TrimStart();
            }
        }

        return chunks;
    }

    public static IEnumerable<string> ChunkByParagraph(this string input, int chunkSize)
    {
        if (input.IsNullOrEmpty())
        {
            return [];
        }
        if (input.Length < chunkSize)
        {
            return [input];
        }
        var chunks = new List<string>();
        var paragraphs = input.ReadParagraphs();
        var currentChunk = new StringBuilder();
        int length = 0;
        foreach (var paragraph in paragraphs)
        {
            var content = paragraph;

            if (length + content.Length >= chunkSize)
            {
                chunks.Add(currentChunk.ToString());
                currentChunk.Clear();
                length = 0;
            }

            if (content.Length > chunkSize)
            {
                var sentences = content.SplitIntoSentences();

                foreach (var sentence in sentences)
                {
                    if (length + sentence.Length >= chunkSize)
                    {
                        chunks.Add(currentChunk.ToString());
                        currentChunk.Clear();
                        length = 0;
                    }
                    currentChunk.Append(sentence);
                    length += sentence.Length;
                }
            }
            else
            {
                currentChunk.AppendLine(paragraph);
                length += paragraph.Length;
            }
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        chunks.RemoveAll(x => x.IsNullOrEmpty());
        return chunks;
    }

    public static IEnumerable<string> SplitIntoSentences(this string content)
    {
        // Use a regular expression to split text into sentences
        string pattern = @"([。！？])"; // @"(?<=[.!?])\s+";
        var result = Regex.Split(content, pattern, RegexOptions.None);
        var sentences = new List<string>();
        for (int i = 0; i < result.Length - 1; i += 2)
        {
            sentences.Add($"{result[i]}{result[i + 1]}");
        }
        return sentences;
    }

    public static IEnumerable<string> Chunk(this string input, int chunkSize)
    {
        if (input.IsNullOrEmpty())
        {
            return [];
        }
        if (input.Length < chunkSize)
        {
            return [input];
        }
        var chunks = new List<string>();
        while (input.IsNotNullOrEmpty())
        {
            int length = Math.Min(input.Length, chunkSize);
            chunks.Add(input[..length]);
            input = input[length..].TrimStart();
        }
        return chunks;
    }

    public static string TruncateParagraph(this string input, int maxLength)
    {
        if (input.Length < maxLength)
        {
            return input;
        }

        // Find the index of the last paragraph break within the maximum length
        int lastParagraphBreakIndex = input.LastIndexOf(Environment.NewLine, maxLength, StringComparison.Ordinal);

        if (lastParagraphBreakIndex == -1)
        {
            return input[..maxLength];
        }

        // Otherwise, truncate the string to the last complete paragraph within the maximum length
        return input[..lastParagraphBreakIndex];
    }

    private static readonly string[] separator = ["\r\n"];

    public static IEnumerable<string> ReadParagraphs(this string content)
    {
        var paragraphs = content.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                .Where(x => x.IsNotNullOrEmpty());

        return paragraphs;
    }

    public static string EscapeRegex(this string input)
    {
        // List of special characters that need to be escaped in a regular expression pattern
        string[] specialChars = ["\\", ".", "+", "*", "?", "^", "$", "(", ")", "[", "]", "{", "}", "|"];

        // Escape each special character with a backslash
        foreach (string specialChar in specialChars)
        {
            input = input.Replace(specialChar, "\\" + specialChar);
        }

        return input;
    }

    public static TEnum GetEnum<TEnum>(string text) where TEnum : struct
    {
        if (!typeof(TEnum).GetTypeInfo().IsEnum)
        {
            throw new InvalidOperationException("Generic parameter 'TEnum' must be an enum.");
        }
        return Enum.Parse<TEnum>(text);
    }

    public static string GetFileSize(this long fileSize)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        int order = 0;
        while (fileSize >= 1024 && order < sizes.Length - 1)
        {
            fileSize /= 1024;
            order++;
        }
        string friendlySize = string.Format("{0:0.#} {1}", fileSize, sizes[order]);
        return friendlySize;
    }

    public static Task<string> ReadFileContent(this string fileName)
    {
        var encoding = fileName.GetEncoding();

        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, encoding);
        var content = reader.ReadToEnd();

        return Task.FromResult(content);
    }

    public static async Task<string> ReadFileContentAsync(this string fileName)
    {
        var encoding = fileName.GetEncoding();

        using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, encoding);
        var content = await reader.ReadToEndAsync();

        return content;
    }

    public static Encoding GetEncoding(this string fileName)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var encodingsToTry = new Encoding[]
            {
                Encoding.UTF8,
                Encoding.Unicode,
                Encoding.GetEncoding("gb2312"), // Add GB2312 encoding to the list
                Encoding.GetEncoding("gbk")     // Add GBK encoding to the list
            };

        Encoding fileEncoding = Encoding.UTF8;
        string fileContents;

        foreach (var encoding in encodingsToTry)
        {
            try
            {
                using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var reader = new StreamReader(stream, encoding, true))
                {
                    fileContents = reader.ReadToEnd();
                }

                if (!fileContents.Any(c => c == '\ufffd')) // Check for invalid characters
                {
                    fileEncoding = encoding;
                    break;
                }
            }
            catch (Exception)
            {
                // Ignore any exceptions and try the next encoding
            }
        }

        return fileEncoding;
    }

    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value);
    }

    public static bool IsNotNullOrEmpty(this string? value)
    {
        return !string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value);
    }

    public static string ExtractExtension(this string extension)
    {
        int index = extension.LastIndexOf('.');
        if (index >= 0)
        {
            return extension[(index + 1)..];
        }

        return extension;
    }

    public static string ToReadableString(this TimeSpan timeSpan)
    {
        return timeSpan.ToString(@"hh\:mm\:ss");
    }

    public static string ToBase64(this string value)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string EncodeXml(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
    }

    public static string ToSsml(this string content, string voiceName, string language)
    {
        var builder = new StringBuilder();

        builder.AppendLine(@$"<speak xmlns=""http://www.w3.org/2001/10/synthesis"" xmlns:mstts=""http://www.w3.org/2001/mstts"" xmlns:emo=""http://www.w3.org/2009/10/emotionml"" version=""1.0"" xml:lang=""{language}"" >");
        builder.AppendLine(@$"<voice name=""{voiceName}"">");

        builder.AppendLine($"{content.EncodeXml()}");

        builder.AppendLine($"</voice>");
        builder.AppendLine($"</speak>");
        return builder.ToString().Replace("\r\n", "\n");
    }

    public static long GetTextCount(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        return value.Length;
    }

    public static string GetFileNameWithoutExtension(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Path.GetFileNameWithoutExtension(value);
    }

    public static string ToLinuxPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
        return path.Replace('\\', '/');
    }

    public static string ToWslPath(this string windowsPath)
    {
        if (string.IsNullOrWhiteSpace(windowsPath))
            throw new ArgumentException("Path cannot be null or empty.", nameof(windowsPath));

        char driveLetter = char.ToLower(windowsPath[0]);
        string pathWithoutDrive = windowsPath[2..].Replace('\\', '/');
        string escapedPath = pathWithoutDrive; //.Replace(" ", "\\ ");
        return $"\"/mnt/{driveLetter}{escapedPath}\"";
    }

    public static string ToWindowsPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var windowsPath = path.Replace('/', '\\');

        // If it's a UNC path or already has a drive letter (like "C:\")
        if (windowsPath.StartsWith("\\\\") ||
            (windowsPath.Length >= 2 && char.IsLetter(windowsPath[0]) && windowsPath[1] == ':'))
        {
            return windowsPath;
        }

        // Otherwise, treat as relative — don't prepend a slash
        return windowsPath;
    }

    public static string RemoveLineBreaks(this string input, string replacement = "")
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return SysLineBreakRegex().Replace(input, replacement);
    }

    [GeneratedRegex(@"\r\n?|\n")]
    private static partial Regex SysLineBreakRegex();

    public static IReadOnlyList<string> ToLines(this string input, int minLength = 36, bool removeEmpty = true)
    {
        if (string.IsNullOrWhiteSpace(input))
            return [];

        input = input.RemoveLineBreaks();
        var regex = LineBreaksRegex();
        var matches = regex.Matches(input);
        var tempSentences = new List<string>();

        foreach (Match match in matches)
        {
            var sentence = match.Value.Trim();
            if (removeEmpty && string.IsNullOrWhiteSpace(sentence))
                continue;

            tempSentences.Add(sentence);
        }

        var result = new List<string>();
        var buffer = new StringBuilder();

        foreach (var sentence in tempSentences)
        {
            if (buffer.Length == 0)
            {
                buffer.Append(sentence);
            }
            else if (buffer.Length + sentence.Length < minLength)
            {
                // Append without breaking
                buffer.Append(sentence);
            }
            else
            {
                // Save current buffer
                result.Add(buffer.ToString());
                buffer.Clear();
                buffer.Append(sentence);
            }
        }

        if (buffer.Length > 0)
            result.Add(buffer.ToString());

        return result.AsReadOnly();
    }

    [GeneratedRegex(@"([^。！？?!，,]+[。！？?!，,])", RegexOptions.Multiline)]
    private static partial Regex LineBreaksRegex();

    public static string RemoveNonPronounceable(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;


        return PronounceRegex().Replace(text, "");
    }

    public static int PronounceableCount(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;


        return text.RemoveNonPronounceable().Length;
    }

    public static int CountWithoutSpaces(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
        return text.Count(c => !char.IsWhiteSpace(c));
    }

    public static double EstimateSpeechDuration(this string text, double secondsPerChar = 0.12)
    {
        var cleanText = text.RemoveNonPronounceable();
        return cleanText.Length * secondsPerChar;
    }

    [GeneratedRegex(@"[^\p{L}\p{N}\u4e00-\u9fff]")]
    private static partial Regex PronounceRegex();

    private static readonly HashSet<char> PronounceablePunctuation =
    [
        ',', '.', ';', ':', '?', '!', '-', '—', '…',

        '，', '。', '；', '：', '？', '！', '、', '“', '”'
    ];

    private static readonly Dictionary<char, double> PunctuationWeights = new()
{
    { ',', 1.3 }, { '.', 1.6 }, { ';', 1.4 }, { ':', 1.4 },
    { '?', 1.8 }, { '!', 1.8 }, { '-', 1.1 }, { '—', 1.3 }, { '…', 2.2 },

    { '，', 1.3 }, { '。', 1.6 }, { '；', 1.4 }, { '：', 1.4 },
    { '？', 1.8 }, { '！', 1.8 }, { '、', 1.2 }, { '“', 1.0 }, { '”', 1.0 }
};

    public static int CountPunctuation(this string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return 0;
        }

        int count = 0;
        foreach (char c in source)
        {
            if (PronounceablePunctuation.Contains(c))
            {
                count++;
            }
        }

        return count;
    }
    public static (int pronounceable, int symbols) CountChars(this string value)
    {
        var symbols = value.CountPunctuation();
        var pronounceable = value.Length - symbols;
        return (pronounceable, symbols);
    }

    public static double GetWeightedLength(this string value, double pronounceableWeight = 1.0)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        double total = 0;
        foreach (char c in value)
        {
            if (PunctuationWeights.TryGetValue(c, out var weight))
            {
                total += weight;
            }
            else
            {
                total += pronounceableWeight;
            }
        }
        return total;
    }
}
