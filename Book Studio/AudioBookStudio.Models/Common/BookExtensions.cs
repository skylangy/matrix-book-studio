using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using System.Text.RegularExpressions;

namespace AudioBookStudio.Models.Common;
public static partial class BookExtensions
{
    public static Book Update(this Book book)
    {
        book.DateUpdated = DateTime.Now;
        book.TextCount = book.Content!.GetTextCount();

        return book;
    }

    public static IEnumerable<Book> ExcludeContent(this IEnumerable<Book> books)
    {
        const int summaryLength = 200;
        foreach (Book book in books)
        {
            if (string.IsNullOrEmpty(book.Summary))
            {
                if (book.Content!.Length >= summaryLength)
                {
                    book.Summary = book.Content[..summaryLength];
                }
                else
                {
                    book.Summary = book.Content;
                }
            }
            book.Content = string.Empty;
            yield return book;
        }
    }

    private static string ExtractChineseNumber(this string chapterTitle)
    {
        var match = ChineseNumberRegex().Match(chapterTitle);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static int ParseChineseNumeral(this string chineseNumeral)
    {
        var chineseNumbers = new Dictionary<char, int>
        {
            { '一', 1 }, { '二', 2 }, { '三', 3 }, { '四', 4 }, { '五', 5 },
            { '六', 6 }, { '七', 7 }, { '八', 8 }, { '九', 9 }, { '十', 10 },
            { '百', 100 }, { '千', 1000 }, { '万', 10000 }, { '亿', 100000000 }
        };

        int result = 0;
        int temp = 0;

        foreach (var ch in chineseNumeral)
        {
            if (chineseNumbers.TryGetValue(ch, out int num))
            {
                if (num < 10)
                {
                    temp += num;
                }
                else
                {
                    if (temp == 0) temp = 1;
                    result += temp * num;
                    temp = 0;
                }
            }
        }

        result += temp;
        return result;
    }

    private static int ChineseChapterSort(string? a, string? b)
    {
        var chapterNumberA = ParseChineseNumeral(ExtractChineseNumber(a ?? string.Empty));
        var chapterNumberB = ParseChineseNumeral(ExtractChineseNumber(b ?? string.Empty));

        if (chapterNumberA < chapterNumberB)
        {
            return -1;
        }
        else if (chapterNumberA > chapterNumberB)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public static List<string> SortNamesByChineseNumeral(this IEnumerable<string> names)
    {
        return [.. names.OrderBy(e => e, Comparer<string?>.Create(ChineseChapterSort))];
    }

    public static List<Chapter> SortChapters(this List<Chapter> chapters)
    {
        return [.. chapters.OrderBy(e => e.Title, Comparer<string?>.Create(ChineseChapterSort))];
    }

    public static IEnumerable<string> GetChapterNames(this Book book)
    {
        return book.Content?.GetChapterNames() ?? [];
    }

    public static IEnumerable<string> GetChapterNames(this string content)
    {
        var pattern = @"
(?<full> 
  (^第[一二三四五六七八九十百零〇\d]+章(?:\s+.+?)?$)
  (?<content>
    [^第后]*(?:(?!(^第[一二三四五六七八九十百零〇\d]+章|后记$))[\s\S])*
  )
)
(?=第[一二三四五六七八九十百零〇\d]+章|$)";
        var regex = new Regex(pattern, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        var matches = regex.Matches(content);


        foreach (Match match in matches)
        {
            string title = match.Groups[1].Value.Trim();
            //string content = match.Groups["content"].Value.Trim();
            yield return title;
        }
    }

    [GeneratedRegex(@"第([\u4e00-\u9fa5]+)[章|回]")]
    private static partial Regex ChineseNumberRegex();
}
