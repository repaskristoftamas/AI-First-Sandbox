using System.Linq.Expressions;

namespace Bookstore.SharedKernel.Specifications;

/// <summary>
/// Encapsulates a reusable, composable query definition for an entity type.
/// </summary>
/// <typeparam name="T">The entity type this specification targets.</typeparam>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// Filter predicate applied as a WHERE clause.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Ascending order-by expression.
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Descending order-by expression.
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Number of records to skip for pagination.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Number of records to take for pagination.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Whether pagination is enabled for this specification.
    /// </summary>
    bool IsPagingEnabled { get; }
}
