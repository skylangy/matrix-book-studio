using System.Text.RegularExpressions;

namespace AudioBookStudio.Common.Shared;

public partial class ChapterNameComparer : IComparer<string>
{
    private const string PrefaceKeyword = "前言";
    private const string EpilogueKeyword = "后记";

    public static readonly ChapterNameComparer Instance = new();

    public int Compare(string x, string y)
    {
        if (x == null || y == null)
        {
            return x == null ? (y == null ? 0 : -1) : 1;
        }

        // Handle special cases for "前言" (Preface) and "后记" (Epilogue)
        if (x.Contains(PrefaceKeyword)) return y.Contains(PrefaceKeyword) ? 0 : -1;
        if (y.Contains(PrefaceKeyword)) return 1;
        if (x.Contains(EpilogueKeyword)) return y.Contains(EpilogueKeyword) ? 0 : 1;
        if (y.Contains(EpilogueKeyword)) return -1;

        int chapterX = ExtractChapterNumber(x);
        int chapterY = ExtractChapterNumber(y);

        return chapterX.CompareTo(chapterY);
    }

    private static int ExtractChapterNumber(string fileName)
    {
        // Regular expression to match the chapter pattern
        var match = ChapterRegex().Match(fileName);
        if (match.Success)
        {
            var chineseNumber = match.Groups["number"].Value;
            return ChineseToNumber(chineseNumber);
        }
        return int.MaxValue; // If no match is found, place at the end
    }


    private static int ChineseToNumber(string chinese)
    {
        var numberMap = new Dictionary<char, int>
        {
            { '一', 1 }, { '二', 2 }, { '三', 3 }, { '四', 4 },
            { '五', 5 }, { '六', 6 }, { '七', 7 }, { '八', 8 },
            { '九', 9 }, { '十', 10 }
        };

        int result = 0;
        int temp = 0;

        foreach (var c in chinese)
        {
            if (c == '十')
            {
                temp = temp == 0 ? 10 : temp * 10;
                result += temp;
                temp = 0;
            }
            else if (numberMap.TryGetValue(c, out int value))
            {
                temp += value;
            }
        }
        result += temp;

        return result;
    }

    [GeneratedRegex(@"第(?<number>[一二三四五六七八九十]+)章")]
    private static partial Regex ChapterRegex();
}
