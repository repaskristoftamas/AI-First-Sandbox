using System.Linq.Expressions;

namespace Bookstore.SharedKernel.Specifications;

/// <summary>
/// Base class for building specifications with a fluent protected API.
/// </summary>
/// <typeparam name="T">The entity type this specification targets.</typeparam>
public abstract class Specification<T> : ISpecification<T> where T : class
{
    /// <inheritdoc />
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc />
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc />
    public int? Skip { get; private set; }

    /// <inheritdoc />
    public int? Take { get; private set; }

    /// <summary>
    /// Sets the filter predicate for this specification.
    /// </summary>
    /// <param name="criteria">Expression to filter entities.</param>
    protected void ApplyCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    /// <summary>
    /// Sets ascending ordering for this specification.
    /// </summary>
    /// <param name="orderBy">Expression to order entities ascending.</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderBy) => OrderBy = orderBy;

    /// <summary>
    /// Sets descending ordering for this specification.
    /// </summary>
    /// <param name="orderByDescending">Expression to order entities descending.</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescending) => OrderByDescending = orderByDescending;

    /// <summary>
    /// Enables pagination with the specified skip and take values.
    /// </summary>
    /// <param name="page">One-based page number.</param>
    /// <param name="pageSize">Number of records per page.</param>
    protected void ApplyPaging(int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        Skip = (page - 1) * pageSize;
        Take = pageSize;
    }
}
