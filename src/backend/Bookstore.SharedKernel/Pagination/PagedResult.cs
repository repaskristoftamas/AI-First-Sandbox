namespace Bookstore.SharedKernel.Pagination;

/// <summary>
/// Represents a single page of results returned from a paginated query, alongside the metadata
/// describing the underlying collection.
/// </summary>
/// <typeparam name="T">The element type of the page.</typeparam>
/// <param name="Items">The items contained in this page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
/// <param name="Page">The one-based index of the current page.</param>
/// <param name="PageSize">The maximum number of items per page.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>
    /// The total number of pages, derived from <see cref="TotalCount"/> and <see cref="PageSize"/>.
    /// Returns zero when there are no items.
    /// </summary>
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// True when at least one additional page follows the current one.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// True when the current page is not the first page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
