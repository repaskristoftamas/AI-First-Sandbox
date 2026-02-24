using Bookstore.Application.Abstractions;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.UpdateBook;

/// <summary>
/// Handles updating a book's properties, enforcing existence and ISBN uniqueness.
/// </summary>
internal sealed class UpdateBookCommandHandler(IApplicationDbContext context) : ICommandHandler<UpdateBookCommand, Result>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Locates the book, verifies no ISBN conflict with other books, and applies the updates.
    /// </summary>
    /// <param name="command">The command containing the updated book properties.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A success result, or a <see cref="NotFoundError"/>/<see cref="ConflictError"/> on failure.</returns>
    public async ValueTask<Result> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (book is null)
            return Result.Failure(new NotFoundError("The book with the specified identifier was not found."));

        bool isbnConflict = await _context.Books
            .AnyAsync(b => b.ISBN == command.ISBN && b.Id != command.Id, cancellationToken);

        if (isbnConflict)
            return Result.Failure(new ConflictError($"A book with ISBN '{command.ISBN}' already exists."));

        book.Update(command.Title, command.Author, command.ISBN, command.Price, command.PublicationYear);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
