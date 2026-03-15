using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Application.Books.Commands.DeleteBook;
using Bookstore.Application.Books.Commands.UpdateBook;
using Bookstore.Application.Books.Queries.GetAllBooks;
using Bookstore.Application.Books.Queries.GetBookById;
using Bookstore.Domain.Books;
using Bookstore.WebApi.Extensions;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.WebApi.Endpoints.Books;

/// <summary>
/// Defines the CRUD endpoints for managing books in the catalog.
/// </summary>
public sealed class BookEndpoints : IEndpointDefinition
{
    /// <summary>
    /// Registers all book-related routes under the /api/books group.
    /// </summary>
    /// <param name="app">The endpoint route builder to register routes on.</param>
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", GetAllBooks)
            .WithName("GetAllBooks")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        group.MapGet("/{id:guid}", GetBookById)
            .WithName("GetBookById")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        group.MapPost("/", CreateBook)
            .WithName("CreateBook")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapPut("/{id:guid}", UpdateBook)
            .WithName("UpdateBook")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapDelete("/{id:guid}", DeleteBook)
            .WithName("DeleteBook")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");
    }

    /// <summary>
    /// Retrieves a page of books from the catalog.
    /// </summary>
    /// <param name="page">One-based page number (default: 1).</param>
    /// <param name="pageSize">Number of results per page (default: 20).</param>
    /// <param name="sender">Mediator sender for dispatching the query.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An OK result with the book page, or a problem response on failure.</returns>
    private static async Task<Results<Ok<List<BookResponse>>, ProblemHttpResult>> GetAllBooks(
        ISender sender,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "ValidationError",
                detail: page < 1
                    ? "'page' must be ≥ 1."
                    : "'pageSize' must be between 1 and 100.");

        var result = await sender.Send(new GetAllBooksQuery(page, pageSize), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.Select(d => d.ToResponse()).ToList())
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Retrieves a single book by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book.</param>
    /// <param name="sender">Mediator sender for dispatching the query.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>An OK result with the book, or a problem response if not found.</returns>
    private static async Task<Results<Ok<BookResponse>, ProblemHttpResult>> GetBookById(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookByIdQuery(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.ToResponse())
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Creates a new book in the catalog from the provided request body.
    /// </summary>
    /// <param name="request">The request body containing the book details.</param>
    /// <param name="sender">Mediator sender for dispatching the command.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A 201 Created result with the new book's identifier, or a problem response on failure.</returns>
    private static async Task<Results<CreatedAtRoute<Guid>, ProblemHttpResult>> CreateBook(
        [FromBody] CreateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateBookCommand(request.Title, request.AuthorId, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetBookById", new { id = result.Value })
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Updates all properties of an existing book identified by the route parameter.
    /// </summary>
    /// <param name="id">The unique identifier of the book to update.</param>
    /// <param name="request">The request body containing the updated book properties.</param>
    /// <param name="sender">Mediator sender for dispatching the command.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A 204 No Content result on success, or a problem response on failure.</returns>
    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBookCommand(new BookId(id), request.Title, request.AuthorId, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Deletes a book from the catalog by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the book to delete.</param>
    /// <param name="sender">Mediator sender for dispatching the command.</param>
    /// <param name="cancellationToken">Token to cancel the request.</param>
    /// <returns>A 204 No Content result on success, or a problem response if not found.</returns>
    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteBook(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookCommand(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }
}
