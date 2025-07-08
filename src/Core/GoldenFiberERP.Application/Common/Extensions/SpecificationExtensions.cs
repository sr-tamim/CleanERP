using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Domain.Specifications;

namespace GoldenFiberERP.Application.Common.Extensions;

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

        // Apply string includes
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

        // Apply group by
        if (specification.GroupBy != null)
        {
            query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
        }

        // Apply paging
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }

    /// <summary>
    /// Apply specification and return total count for paging
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="query">Base query</param>
    /// <param name="specification">Specification to apply</param>
    /// <returns>Tuple with filtered query and total count</returns>
    public static async Task<(IQueryable<T> Query, int TotalCount)> ApplySpecificationWithCountAsync<T>(
        this IQueryable<T> query, 
        ISpecification<T> specification)
        where T : class
    {
        // Get total count before applying paging
        var countQuery = query;
        if (specification.Criteria != null)
        {
            countQuery = countQuery.Where(specification.Criteria);
        }
        
        var totalCount = await countQuery.CountAsync();
        
        // Apply full specification
        var resultQuery = query.ApplySpecification(specification);
        
        return (resultQuery, totalCount);
    }
}
