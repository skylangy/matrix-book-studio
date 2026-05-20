namespace AudioBookStudio.Common.Shared;
public class BookStatusComparer : IComparer<string>
{
    public static readonly BookStatusComparer Instance = new();

    public int Compare(string x, string y)
    {
        if (x == y)
            return 0;

        if (x == BookStatus.Finished)
            return 1;

        return -1;
    }
}
