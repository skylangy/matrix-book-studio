namespace AudioBookStudio.Common.Shared;
public static class PagedListExtensions
{
    public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> superset, int index, int pageSize)
    {
        if (index < 1)
            index = 1;
        return new PagedList<T>(superset, index - 1, pageSize);
    }

    public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> superset, int index, int pageSize, int total)
    {
        if (index < 1)
            index = 1;
        return new PagedList<T>(superset, index - 1, pageSize, total);
    }
}
