using Bookstore.SharedKernel.Results;

namespace Bookstore.WebApi.Extensions;

public static class ErrorExtensions
{
    public static IResult ToProblemResult(this Error error) => error switch
    {
        NotFoundError e   => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "NotFound", detail: e.Description),
        ConflictError e   => Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: e.Description),
        ValidationError e => Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "ValidationError", detail: e.Description),
        _                 => Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "InternalError", detail: error.Description)
    };
}
