using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

internal sealed class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, Result<IReadOnlyList<BookDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllBooksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<BookDto>>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await _context.Books
            .AsNoTracking()
            .Select(b => new BookDto(b.Id, b.Title, b.Author, b.ISBN, b.Price, b.PublicationYear))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BookDto>>(books);
    }
}
