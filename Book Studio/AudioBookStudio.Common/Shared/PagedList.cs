namespace AudioBookStudio.Common.Shared;
/// <summary>
/// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
/// </summary>
/// <remarks>
/// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
/// </remarks>
/// <typeparam name="T">The type of object the collection should contain.</typeparam>
/// <seealso cref="IPagedList{T}"/>
/// <seealso cref="List{T}"/>
public class PagedList<T> : IPagedList<T>
{
    private readonly List<T> mInnerList = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{T}"/> class that divides the supplied superset into subsets the size of the supplied pageSize. The instance then only containes the objects contained in the subset specified by index.
    /// </summary>
    /// <param name="superset">The collection of objects to be divided into subsets. If the collection implements <see cref="IQueryable{T}"/>, it will be treated as such.</param>
    /// <param name="index">The index of the subset of objects to be contained by this instance.</param>
    /// <param name="pageSize">The maximum size of any individual subset.</param>
    /// <exception cref="ArgumentOutOfRangeException">The specified index cannot be less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The specified page size cannot be less than one.</exception>
    public PagedList(IEnumerable<T> superset, int index, int pageSize)
    {
        // set source to blank list if superset is null to prevent exceptions
        var source = superset == null
                                ? new List<T>().AsQueryable()
                                : superset.AsQueryable();

        TotalItemCount = source.Count();
        PageSize = pageSize;
        PageIndex = index;
        if (TotalItemCount > 0)
            PageCount = (int)Math.Ceiling(TotalItemCount / (double)PageSize);
        else
            PageCount = 0;

        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, "PageIndex cannot be below 0.");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");


        // add items to internal list
        if (TotalItemCount > 0)
        {
            if (index == 0)
                mInnerList.AddRange([.. source.Take(pageSize)]);
            else
                mInnerList.AddRange([.. source.Skip((index) * pageSize).Take(pageSize)]);
        }
    }

    public PagedList(IEnumerable<T> source, int index, int pageSize, int total)
    {
        TotalItemCount = total;
        PageIndex = index;
        PageSize = pageSize;

        if (TotalItemCount > 0)
            PageCount = (int)Math.Ceiling(TotalItemCount / (double)PageSize);
        else
            PageCount = 0;

        mInnerList.AddRange([.. source]);
    }

    /// <summary>
    /// Gets the items.
    /// </summary>
    /// <value>
    /// The items.
    /// </value>
    public IList<T> Items => mInnerList;

    /// <summary>
    /// Total number of subsets within the superset.
    /// </summary>
    /// <value>
    /// Total number of subsets within the superset.
    /// </value>
    public int PageCount { get; private set; }
    /// <summary>
    /// Total number of objects contained within the superset.
    /// </summary>
    /// <value>
    /// Total number of objects contained within the superset.
    /// </value>
    public int TotalItemCount { get; private set; }
    /// <summary>
    /// Zero-based index of this subset within the superset.
    /// </summary>
    /// <value>
    /// Zero-based index of this subset within the superset.
    /// </value>
    public int PageIndex { get; private set; }
    /// <summary>
    /// One-based index of this subset within the superset.
    /// </summary>
    /// <value>
    /// One-based index of this subset within the superset.
    /// </value>
    public int PageNumber => PageIndex + 1;

    /// <summary>
    /// Maximum size any individual subset.
    /// </summary>
    /// <value>
    /// Maximum size any individual subset.
    /// </value>
    public int PageSize { get; private set; }
    /// <summary>
    /// Returns true if this is NOT the first subset within the superset.
    /// </summary>
    /// <value>
    /// Returns true if this is NOT the first subset within the superset.
    /// </value>
    public bool HasPreviousPage => PageIndex > 0;

    /// <summary>
    /// Returns true if this is NOT the last subset within the superset.
    /// </summary>
    /// <value>
    /// Returns true if this is NOT the last subset within the superset.
    /// </value>
    public bool HasNextPage => PageIndex < (PageCount - 1);

    /// <summary>
    /// Returns true if this is the first subset within the superset.
    /// </summary>
    /// <value>
    /// Returns true if this is the first subset within the superset.
    /// </value>
    public bool IsFirstPage => PageIndex <= 0;

    /// <summary>
    /// Returns true if this is the last subset within the superset.
    /// </summary>
    /// <value>
    /// Returns true if this is the last subset within the superset.
    /// </value>
    public bool IsLastPage => PageIndex >= (PageCount - 1);
}
