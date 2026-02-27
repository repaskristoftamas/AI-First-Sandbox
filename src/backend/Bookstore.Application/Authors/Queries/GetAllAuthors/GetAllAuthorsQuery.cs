using Bookstore.Application.Authors.DTOs;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Queries.GetAllAuthors;

/// <summary>
/// Query to retrieve all authors in the catalog.
/// </summary>
public sealed record GetAllAuthorsQuery : IQuery<Result<IReadOnlyList<AuthorDto>>>;
