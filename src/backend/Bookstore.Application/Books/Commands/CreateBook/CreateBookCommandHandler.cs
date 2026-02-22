using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.CreateBook;

internal sealed class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateBookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        bool isbnExists = await _context.Books
            .AnyAsync(b => b.ISBN == request.ISBN, cancellationToken);

        if (isbnExists)
            return Result.Failure<Guid>(BookErrors.Conflict(request.ISBN));

        var book = Book.Create(
            request.Title,
            request.Author,
            request.ISBN,
            request.Price,
            request.PublicationYear);

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(book.Id);
    }
}
