using Bookstore.Application.Abstractions;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.DeleteBook;

/// <summary>
/// Handles deletion of a book, returning a not-found error if the book does not exist.
/// </summary>
internal sealed class DeleteBookCommandHandler(IApplicationDbContext context) : ICommandHandler<DeleteBookCommand, Result>
{
    private readonly IApplicationDbContext _context = context;

    /// <summary>
    /// Locates the book by identifier and removes it from the catalog.
    /// </summary>
    public async ValueTask<Result> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (book is null)
            return Result.Failure(new NotFoundError("The book with the specified identifier was not found."));

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
