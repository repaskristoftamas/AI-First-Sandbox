using Bookstore.Application.Books.DTOs;
using Bookstore.SharedKernel.Results;
using MediatR;

namespace Bookstore.Application.Books.Queries.GetAllBooks;

public sealed record GetAllBooksQuery : IRequest<Result<IReadOnlyList<BookDto>>>;
