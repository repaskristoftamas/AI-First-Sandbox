using Bookstore.SharedKernel.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Bookstore.WebApi.Extensions;

public static class ErrorExtensions
{
    public static ProblemHttpResult ToProblemHttpResult(this Error error)
    {
        var (statusCode, title) = error switch
        {
            NotFoundError   => (StatusCodes.Status404NotFound, "NotFound"),
            ConflictError   => (StatusCodes.Status409Conflict, "Conflict"),
            ValidationError => (StatusCodes.Status400BadRequest, "ValidationError"),
            _               => (StatusCodes.Status500InternalServerError, "InternalError")
        };

        return TypedResults.Problem(statusCode: statusCode, title: title, detail: error.Description);
    }
}
