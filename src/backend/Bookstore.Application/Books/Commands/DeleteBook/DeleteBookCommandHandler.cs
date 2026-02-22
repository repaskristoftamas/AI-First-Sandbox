using Bookstore.Application.Abstractions;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Commands.DeleteBook;

internal sealed class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteBookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (book is null)
            return Result.Failure(BookErrors.NotFound);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
