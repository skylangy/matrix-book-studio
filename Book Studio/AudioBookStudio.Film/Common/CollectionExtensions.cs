namespace AudioBookStudio.Films.Common;
public static class CollectionExtensions
{
    public static bool HasItems<T>(this IList<T> list)
    {
        return list != null && list.Count > 0;
    }
}
