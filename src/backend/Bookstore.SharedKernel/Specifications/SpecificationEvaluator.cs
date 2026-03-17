namespace Bookstore.SharedKernel.Specifications;

/// <summary>
/// Applies an <see cref="ISpecification{T}"/> to an <see cref="IQueryable{T}"/>,
/// building the final query with criteria, ordering, and pagination.
/// </summary>
public static class SpecificationEvaluator
{
    /// <summary>
    /// Applies the given specification to the queryable source, returning the composed query.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <param name="source">The base queryable to apply the specification against.</param>
    /// <param name="specification">The specification defining criteria, ordering, and pagination.</param>
    /// <returns>The queryable with all specification clauses applied.</returns>
    public static IQueryable<T> Apply<T>(IQueryable<T> source, ISpecification<T> specification) where T : class
    {
        var query = source;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
            {
                query = query.Skip(specification.Skip.Value);
            }

            if (specification.Take.HasValue)
            {
                query = query.Take(specification.Take.Value);
            }
        }

        return query;
    }
}
