using Bookstore.Application.Abstractions;
using Bookstore.Application.Extensions;
using Bookstore.Domain.Authors;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using FluentValidation;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.UpdateBook;

/// <summary>
/// Handles updating a book's properties.
/// </summary>
/// <remarks>
/// Enforces existence, author existence, and ISBN uniqueness before applying changes.
/// </remarks>
internal sealed class UpdateBookCommandHandler(
    IApplicationDbContext context,
    IValidator<UpdateBookCommand> validator,
    TimeProvider timeProvider) : ICommandHandler<UpdateBookCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IValidator<UpdateBookCommand> _validator = validator;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Applies the updated properties to an existing book.
    /// </summary>
    /// <remarks>
    /// Locates the book by identifier, verifies the author exists, and checks for ISBN conflicts before applying updates.
    /// </remarks>
    /// <param name="command">The command containing the updated book properties.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A success result, or a failure on not-found/conflict errors.</returns>
    public async ValueTask<Result> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToFailureResult();

        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (book is null)
            return Result.Failure(new NotFoundError(BookErrorCodes.NotFound, "The book with the specified identifier was not found."));

        var authorId = new AuthorId(command.AuthorId);

        bool authorExists = await _context.Authors
            .AnyAsync(a => a.Id == authorId, cancellationToken);

        if (!authorExists)
            return Result.Failure(new NotFoundError(BookErrorCodes.AuthorNotFound, "The author with the specified identifier was not found."));

        var isbnResult = Isbn.Create(command.ISBN);
        if (isbnResult.IsFailure)
            return Result.Failure(isbnResult.Error);

        var isbn = isbnResult.Value;

        bool isbnConflict = await _context.Books
            .AnyAsync(b => b.ISBN == isbn && b.Id != command.Id, cancellationToken);

        if (isbnConflict)
            return Result.Failure(new ConflictError(BookErrorCodes.IsbnConflict, $"A book with ISBN '{command.ISBN}' already exists."));

        var updateResult = book.Update(command.Title, authorId, isbn, command.Price, command.PublicationYear, _timeProvider);
        if (updateResult.IsFailure)
            return updateResult;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
