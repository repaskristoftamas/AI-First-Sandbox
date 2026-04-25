namespace Bookstore.WebApi.Pagination;

/// <summary>
/// API response wrapper that returns a page of items alongside pagination metadata.
/// </summary>
/// <typeparam name="T">The element type of the page.</typeparam>
/// <param name="Items">The items contained in this page.</param>
/// <param name="TotalCount" example="42">The total number of items across all pages.</param>
/// <param name="Page" example="1">The one-based index of the current page.</param>
/// <param name="PageSize" example="20">The maximum number of items per page.</param>
/// <param name="TotalPages" example="3">The total number of pages.</param>
/// <param name="HasNextPage" example="true">Whether at least one more page follows the current one.</param>
/// <param name="HasPreviousPage" example="false">Whether the current page is preceded by another page.</param>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);
