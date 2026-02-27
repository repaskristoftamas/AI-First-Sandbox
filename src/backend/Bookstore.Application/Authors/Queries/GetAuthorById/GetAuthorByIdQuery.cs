using Bookstore.Application.Authors.DTOs;
using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Queries.GetAuthorById;

/// <summary>
/// Query to retrieve a single author by their identifier.
/// </summary>
/// <param name="Id">Identifier of the author to retrieve.</param>
public sealed record GetAuthorByIdQuery(AuthorId Id) : IQuery<Result<AuthorDto>>;
