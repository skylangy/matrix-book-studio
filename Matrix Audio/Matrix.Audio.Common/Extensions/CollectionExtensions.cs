using System.Collections;

namespace Matrix.Audio.Common.Extensions;
public static class CollectionExtensions
{
    public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
    {
        foreach (var item in values)
        {
            action(item);
        }
    }

    public static int Count(this IEnumerable values)
    {
        var count = 0;
        if (values == null)
        {
            return count;
        }

        foreach (var item in values)
        {
            count++;
        }
        return count;
    }
}
