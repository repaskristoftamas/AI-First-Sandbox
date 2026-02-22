using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.CreateBook;

internal sealed class CreateBookCommandHandler(IApplicationDbContext context) : ICommandHandler<CreateBookCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context = context;

    public async ValueTask<Result<Guid>> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        bool isbnExists = await _context.Books
            .AnyAsync(b => b.ISBN == command.ISBN, cancellationToken);

        if (isbnExists)
            return Result.Failure<Guid>(new ConflictError($"A book with ISBN '{command.ISBN}' already exists."));

        var book = Book.Create(
            command.Title,
            command.Author,
            command.ISBN,
            command.Price,
            command.PublicationYear);

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(book.Id.Value);
    }
}
