using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.DeleteBook;

/// <summary>
/// Handles deletion of a book.
/// </summary>
/// <remarks>
/// Returns a not-found error if the book does not exist.
/// </remarks>
internal sealed class DeleteBookCommandHandler(IApplicationDbContext context, TimeProvider timeProvider) : ICommandHandler<DeleteBookCommand, Result>
{
    private readonly IApplicationDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;

    /// <summary>
    /// Locates the book by identifier and soft-deletes it.
    /// </summary>
    /// <param name="command">The command containing the identifier of the book to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A success result, or a <see cref="NotFoundError"/> if the book does not exist.</returns>
    public async ValueTask<Result> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);

        if (book is null)
            return Result.Failure(new NotFoundError(BookErrorCodes.NotFound, "The book with the specified identifier was not found."));

        book.Delete(_timeProvider);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
