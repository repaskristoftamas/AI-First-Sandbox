using Bookstore.Domain.Books;
using Bookstore.SharedKernel.Specifications;

namespace Bookstore.Application.Books.Specifications;

/// <summary>
/// Specification for retrieving a paginated, ordered list of books.
/// </summary>
internal sealed class PaginatedBooksSpecification : Specification<Book>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedBooksSpecification"/> class.
    /// </summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Number of books per page.</param>
    public PaginatedBooksSpecification(int page, int pageSize)
    {
        ApplyOrderBy(b => b.Id.Value);
        ApplyPaging(page, pageSize);
    }
}
