using Bookstore.Application.Authors.DTOs;
using Bookstore.SharedKernel.Results;
using Mediator;

namespace Bookstore.Application.Authors.Queries.GetAllAuthors;

/// <summary>
/// Query to retrieve a page of authors from the catalog.
/// </summary>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Number of authors per page.</param>
public sealed record GetAllAuthorsQuery(int Page = 1, int PageSize = 20) : IQuery<Result<IReadOnlyList<AuthorDto>>>;
