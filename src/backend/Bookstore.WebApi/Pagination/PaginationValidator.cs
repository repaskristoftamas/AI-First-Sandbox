using Microsoft.AspNetCore.Http.HttpResults;

namespace Bookstore.WebApi.Pagination;

/// <summary>
/// Provides shared validation for pagination query parameters at the API boundary.
/// </summary>
public static class PaginationValidator
{
    private const int MaxPageSize = 100;

    /// <summary>
    /// Validates the supplied page and pageSize parameters, returning a <see cref="ProblemHttpResult"/>
    /// when they are outside acceptable bounds, or <c>null</c> when valid.
    /// </summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A <see cref="ProblemHttpResult"/> describing the first invalid parameter, or <c>null</c> if valid.</returns>
    public static ProblemHttpResult? Validate(int page, int pageSize)
    {
        if (page < 1)
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "ValidationError",
                detail: "'page' must be ≥ 1.");

        if (pageSize < 1 || pageSize > MaxPageSize)
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "ValidationError",
                detail: "'pageSize' must be between 1 and 100.");

        return null;
    }
}
