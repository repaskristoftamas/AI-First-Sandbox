using Bookstore.SharedKernel.Pagination;

namespace Bookstore.WebApi.Pagination;

/// <summary>
/// API response wrapper that returns a page of items alongside pagination metadata.
/// Derived fields (<see cref="TotalPages"/>, <see cref="HasNextPage"/>, <see cref="HasPreviousPage"/>)
/// are computed from the underlying <see cref="PagedResult{T}"/> to prevent inconsistent data.
/// </summary>
/// <typeparam name="T">The element type of the page.</typeparam>
public sealed record PagedResponse<T>
{
    /// <summary>The items contained in this page.</summary>
    public required IReadOnlyList<T> Items { get; init; }

    /// <summary>The total number of items across all pages.</summary>
    public required int TotalCount { get; init; }

    /// <summary>The one-based index of the current page.</summary>
    public required int Page { get; init; }

    /// <summary>The maximum number of items per page.</summary>
    public required int PageSize { get; init; }

    /// <summary>The total number of pages.</summary>
    public required int TotalPages { get; init; }

    /// <summary>Whether at least one more page follows the current one.</summary>
    public required bool HasNextPage { get; init; }

    /// <summary>Whether the current page is preceded by another page.</summary>
    public required bool HasPreviousPage { get; init; }

    /// <summary>
    /// Creates a <see cref="PagedResponse{T}"/> from a <see cref="PagedResult{TSource}"/>,
    /// mapping items with the supplied selector and deriving pagination metadata.
    /// </summary>
    /// <typeparam name="TSource">The source element type in the paged result.</typeparam>
    /// <param name="pagedResult">The paged result to convert.</param>
    /// <param name="selector">A function that maps each source item to the response type.</param>
    /// <returns>A new <see cref="PagedResponse{T}"/> with consistent metadata.</returns>
    public static PagedResponse<T> FromPagedResult<TSource>(PagedResult<TSource> pagedResult, Func<TSource, T> selector)
    {
        return new PagedResponse<T>
        {
            Items = [.. pagedResult.Items.Select(selector)],
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            HasNextPage = pagedResult.HasNextPage,
            HasPreviousPage = pagedResult.HasPreviousPage
        };
    }
}
