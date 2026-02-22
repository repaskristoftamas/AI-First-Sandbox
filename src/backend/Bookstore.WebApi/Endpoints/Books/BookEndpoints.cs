using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Application.Books.Commands.DeleteBook;
using Bookstore.Application.Books.Commands.UpdateBook;
using Bookstore.Application.Books.Queries.GetAllBooks;
using Bookstore.Application.Books.Queries.GetBookById;
using Bookstore.Domain.Books;
using Bookstore.WebApi.Extensions;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.WebApi.Endpoints.Books;

public sealed class BookEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", GetAllBooks)
            .WithName("GetAllBooks")
            .Produces<IReadOnlyList<BookResponse>>();

        group.MapGet("/{id:guid}", GetBookById)
            .WithName("GetBookById")
            .Produces<BookResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBook)
            .WithName("CreateBook")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateBook)
            .WithName("UpdateBook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteBook)
            .WithName("DeleteBook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllBooks(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllBooksQuery(), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value.Select(d => d.ToResponse()).ToList())
            : result.Error.ToProblemResult();
    }

    private static async Task<IResult> GetBookById(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookByIdQuery(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? Results.Ok(result.Value.ToResponse())
            : result.Error.ToProblemResult();
    }

    private static async Task<IResult> CreateBook(
        [FromBody] CreateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateBookCommand(request.Title, request.Author, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("GetBookById", new { id = result.Value }, result.Value)
            : result.Error.ToProblemResult();
    }

    private static async Task<IResult> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBookCommand(new BookId(id), request.Title, request.Author, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : result.Error.ToProblemResult();
    }

    private static async Task<IResult> DeleteBook(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookCommand(new BookId(id)), cancellationToken);
        return result.IsSuccess
            ? Results.NoContent()
            : result.Error.ToProblemResult();
    }
}
