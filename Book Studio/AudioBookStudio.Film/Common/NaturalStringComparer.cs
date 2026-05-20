using System.Text.RegularExpressions;

namespace AudioBookStudio.Films.Common;
public partial class NaturalStringComparer : IComparer<string>
{
    //private static readonly Regex _numberRegex = NumberRegex();

    public int Compare(string a, string b)
    {
        var aParts = NumberRegex().Split(a);
        var aNumbers = NumberRegex().Matches(a).Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

        var bParts = NumberRegex().Split(b);
        var bNumbers = NumberRegex().Matches(b).Cast<Match>().Select(m => int.Parse(m.Value)).ToList();

        for (int i = 0; i < Math.Min(aParts.Length, bParts.Length); i++)
        {
            int textCompare = string.Compare(aParts[i], bParts[i], StringComparison.OrdinalIgnoreCase);
            if (textCompare != 0)
                return textCompare;

            if (i < aNumbers.Count && i < bNumbers.Count)
            {
                int numCompare = aNumbers[i].CompareTo(bNumbers[i]);
                if (numCompare != 0)
                    return numCompare;
            }
        }

        return a.Length.CompareTo(b.Length);
    }

    [GeneratedRegex(@"\d+", RegexOptions.Compiled)]
    private static partial Regex NumberRegex();
}

