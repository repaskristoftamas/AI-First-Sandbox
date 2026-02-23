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
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", GetAllBooks)
            .WithName("GetAllBooks");

        group.MapGet("/{id:guid}", GetBookById)
            .WithName("GetBookById");

        group.MapPost("/", CreateBook)
            .WithName("CreateBook");

        group.MapPut("/{id:guid}", UpdateBook)
            .WithName("UpdateBook");

        group.MapDelete("/{id:guid}", DeleteBook)
            .WithName("DeleteBook");
    }

    /// <summary>
    /// Retrieves all books in the catalog.
    /// </summary>
    private static async Task<Results<Ok<List<BookResponse>>, ProblemHttpResult>> GetAllBooks(
        ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllBooksQuery(), cancellationToken);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value.Select(d => d.ToResponse()).ToList())
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Retrieves a single book by its identifier.
    /// </summary>
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
    private static async Task<Results<CreatedAtRoute<Guid>, ProblemHttpResult>> CreateBook(
        [FromBody] CreateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateBookCommand(request.Title, request.Author, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(result.Value, "GetBookById", new { id = result.Value })
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Updates all properties of an existing book identified by the route parameter.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBookCommand(new BookId(id), request.Title, request.Author, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }

    /// <summary>
    /// Deletes a book from the catalog by its identifier.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteBook(
        Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookCommand(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error.ToProblemHttpResult();
    }
}
