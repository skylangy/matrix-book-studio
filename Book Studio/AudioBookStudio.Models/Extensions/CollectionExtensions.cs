using System.Collections.Concurrent;

namespace AudioBookStudio.Models.Extensions;

public static class CollectionExtensions
{
    public static int RemoveAll<T>(this IList<T> source, Predicate<T> match)
    {
        ArgumentNullException.ThrowIfNull(match);

        int count = 0;
        for (int i = source.Count - 1; i >= 0; i--)
        {
            if (match(source[i]))
            {
                source.RemoveAt(i);
                count++;
            }
        }
        return count;
    }

    public static ConcurrentQueue<T> Enqueue<T>(this ConcurrentQueue<T> source, ConcurrentQueue<T> values)
    {
        while (values.TryDequeue(out var value))
        {
            source.Enqueue(value);
        }
        return source;
    }

    public static ConcurrentQueue<T> Join<T>(this ConcurrentQueue<T> source, ConcurrentQueue<T> values)
    {
        foreach (var value in values)
        {
            source.Enqueue(value);
        }
        return source;
    }

    public static ConcurrentQueue<T> ToQueue<T>(this IEnumerable<T> source)
    {
        return new ConcurrentQueue<T>(source);
    }

    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return !source.Any();
    }

    public static bool IsNotEmpty<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Any();
    }
}