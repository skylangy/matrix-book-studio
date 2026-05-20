namespace AudioBookStudio.Models.Data;

/// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
/// </summary>
/// <remarks>
/// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
/// </remarks>
/// <typeparam name="T">The type of object the collection should contain.</typeparam>
/// <seealso cref="IList{T}"/>
public interface IPagedList<T>
{

    IList<T> Items { get; }

    int PageCount { get; }

    int TotalItemCount { get; }

    int PageIndex { get; }

    int PageNumber { get; }


    int PageSize { get; }


    bool HasPreviousPage { get; }


    bool HasNextPage { get; }


    bool IsFirstPage { get; }


    bool IsLastPage { get; }
}