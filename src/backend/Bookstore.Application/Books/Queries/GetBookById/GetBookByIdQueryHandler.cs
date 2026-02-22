using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetBookById;

internal sealed class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBookByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BookDto>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (book is null)
            return Result.Failure<BookDto>(BookErrors.NotFound);

        return Result.Success(new BookDto(book.Id, book.Title, book.Author, book.ISBN, book.Price, book.PublicationYear));
    }
}
