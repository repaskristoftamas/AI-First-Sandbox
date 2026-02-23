using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.Application.Books.Mappers;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

internal sealed class GetAllBooksQueryHandler(IApplicationDbContext context) : IQueryHandler<GetAllBooksQuery, Result<IReadOnlyList<BookDto>>>
{
    private readonly IApplicationDbContext _context = context;

    public async ValueTask<Result<IReadOnlyList<BookDto>>> Handle(GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        var books = await _context.Books
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<BookDto>>([.. books.Select(b => b.ToDto())]);
    }
}
