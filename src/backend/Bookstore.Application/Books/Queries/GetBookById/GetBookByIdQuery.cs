using Bookstore.Application.Books.DTOs;
using Bookstore.SharedKernel.Results;
using MediatR;

namespace Bookstore.Application.Books.Queries.GetBookById;

public sealed record GetBookByIdQuery(Guid Id) : IRequest<Result<BookDto>>;
