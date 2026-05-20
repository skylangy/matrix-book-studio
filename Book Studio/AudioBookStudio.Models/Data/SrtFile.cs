using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioBookStudio.Models.Data;
public partial class SrtFile
{
    public List<SrtLine> Lines { get; set; } = [];

    public static SrtFile Load(string filePath)
    {
        var srt = new SrtFile();
        var lines = File.ReadAllLines(filePath);
        var block = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue; // skip empty lines

            if (int.TryParse(line.Trim(), out int _)) // likely start of a new block
            {
                if (block.Count > 0)
                {
                    try
                    {
                        srt.Lines.Add(ParseBlock(block));
                    }
                    catch (FormatException ex)
                    {
                        throw new FormatException($"Error parsing block: {string.Join(" | ", block)}", ex);
                    }
                    block.Clear();
                }
            }


            block.Add(line.TrimEnd());
        }

        if (block.Count > 0)
        {
            try
            {
                srt.Lines.Add(ParseBlock(block));
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Error parsing last block: {string.Join(" | ", block)}", ex);
            }
        }

        return srt;
    }

    public void Save(string filePath)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < Lines.Count; i++)
        {
            Lines[i].Index = i + 1;
            builder.AppendLine(Lines[i].ToString());
        }

        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }

    public void ShiftAll(TimeSpan offset)
    {
        foreach (var line in Lines)
        {
            line.Shift(offset);
        }
    }

    private static SrtLine ParseBlock(List<string> block)
    {
        if (block.Count < 2)
            throw new FormatException("Invalid SRT block: not enough lines.");

        var srtLine = new SrtLine();

        // Try parse index
        if (!int.TryParse(block[0], out int index))
            throw new FormatException($"Invalid SRT block index: {block[0]}");

        srtLine.Index = index;

        // Try parse timestamps
        var timestampRegex = LineRegex();

        var match = timestampRegex.Match(block[1]);
        if (!match.Success)
            throw new FormatException($"Invalid time format in block index {index}: {block[1]}");

        srtLine.Start = TimeSpan.ParseExact(match.Groups["start"].Value, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
        srtLine.End = TimeSpan.ParseExact(match.Groups["end"].Value, @"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);

        // Text lines
        for (int i = 2; i < block.Count; i++)
            srtLine.TextLines.Add(block[i]);

        return srtLine;
    }

    [GeneratedRegex(@"(?<start>\d{2}:\d{2}:\d{2},\d{3})\s*-->\s*(?<end>\d{2}:\d{2}:\d{2},\d{3})")]
    private static partial Regex LineRegex();
}
