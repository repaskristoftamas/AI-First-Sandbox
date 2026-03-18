using Bookstore.Domain.Authors;
using Bookstore.SharedKernel.Specifications;

namespace Bookstore.Application.Authors.Specifications;

/// <summary>
/// Specification for retrieving a paginated, ordered list of authors.
/// </summary>
internal sealed class PaginatedAuthorsSpecification : Specification<Author>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedAuthorsSpecification"/> class.
    /// </summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Number of authors per page.</param>
    public PaginatedAuthorsSpecification(int page, int pageSize)
    {
        ApplyOrderBy(a => a.CreatedAt);
        ApplyPaging(page, pageSize);
    }
}
