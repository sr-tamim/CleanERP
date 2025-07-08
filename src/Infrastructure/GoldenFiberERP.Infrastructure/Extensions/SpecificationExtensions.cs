using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Specifications;

namespace GoldenFiberERP.Infrastructure.Extensions;

/// <summary>
/// Extension methods for applying specifications to IQueryable
/// </summary>
public static class SpecificationExtensions
{
    /// <summary>
    /// Apply a specification to an IQueryable
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Base query</param>
    /// <param name="specification">Specification to apply</param>
    /// <returns>Query with specification applied</returns>
    public static IQueryable<T> ApplySpecification<T>(this IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        // Apply criteria
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply includes
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ordering
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply grouping
        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(g => g);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }

    /// <summary>
    /// Get count of entities that match the specification criteria (without paging)
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Base query</param>
    /// <param name="specification">Specification to apply</param>
    /// <returns>Count query</returns>
    public static IQueryable<T> ApplySpecificationForCount<T>(this IQueryable<T> query, ISpecification<T> specification)
        where T : class
    {
        // Only apply criteria for count, skip ordering and paging
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        return query;
    }
}
