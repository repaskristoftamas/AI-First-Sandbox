using Bookstore.Application.Abstractions;
using Bookstore.Application.Books.DTOs;
using Bookstore.Application.Books.Mappers;
using Bookstore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Books.Queries.GetBookById;

internal sealed class GetBookByIdQueryHandler(IApplicationDbContext context) : IQueryHandler<GetBookByIdQuery, Result<BookDto>>
{
    private readonly IApplicationDbContext _context = context;

    public async ValueTask<Result<BookDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken);

        if (book is null)
            return Result.Failure<BookDto>(new NotFoundError("The book with the specified identifier was not found."));

        return Result.Success(book.ToDto());
    }
}
