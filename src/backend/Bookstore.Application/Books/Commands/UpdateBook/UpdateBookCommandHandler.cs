using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.UpdateBook;

internal sealed class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateBookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (book is null)
            return Result.Failure(BookErrors.NotFound);

        bool isbnConflict = await _context.Books
            .AnyAsync(b => b.ISBN == request.ISBN && b.Id != request.Id, cancellationToken);

        if (isbnConflict)
            return Result.Failure(BookErrors.Conflict(request.ISBN));

        book.Update(request.Title, request.Author, request.ISBN, request.Price, request.PublicationYear);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
