using Bookstore.Application.Books.Commands.CreateBook;
using Bookstore.Application.Books.Commands.DeleteBook;
using Bookstore.Application.Books.Commands.UpdateBook;
using Bookstore.Application.Books.DTOs;
using Bookstore.Application.Books.Queries.GetAllBooks;
using Bookstore.Application.Books.Queries.GetBookById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.WebApi.Endpoints.Books;

public sealed class BookEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", GetAllBooks)
            .WithName("GetAllBooks")
            .Produces<IReadOnlyList<BookDto>>();

        group.MapGet("/{id:guid}", GetBookById)
            .WithName("GetBookById")
            .Produces<BookDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateBook)
            .WithName("CreateBook")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", UpdateBook)
            .WithName("UpdateBook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteBook)
            .WithName("DeleteBook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllBooks(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllBooksQuery(), cancellationToken);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetBookById(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBookByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> CreateBook(
        [FromBody] CreateBookCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess
            ? Results.CreatedAtRoute("GetBookById", new { id = result.Value }, result.Value)
            : Results.BadRequest(new { result.Error.Code, result.Error.Description });
    }

    private static async Task<IResult> UpdateBook(
        Guid id,
        [FromBody] UpdateBookRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBookCommand(id, request.Title, request.Author, request.ISBN, request.Price, request.PublicationYear);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeleteBook(Guid id, ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteBookCommand(id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }
}

public sealed record UpdateBookRequest(
    string Title,
    string Author,
    string ISBN,
    decimal Price,
    int PublicationYear);
