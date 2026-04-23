using Microsoft.EntityFrameworkCore;
using CleanERP.Domain.Entities.Common;
using CleanERP.Domain.Interfaces.Repositories.Common;
using CleanERP.Domain.Specifications;
using CleanERP.Persistence.Contexts;

namespace CleanERP.Persistence.Repositories.Common;

/// <summary>
/// Base repository implementation providing common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await _context.Set<T>().AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        return await Task.FromResult(entity);
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _context.Set<T>().Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Apply a specification to a query
    /// </summary>
    protected virtual IQueryable<T> ApplySpecification(ISpecification<T> specification)
    {
        var query = _context.Set<T>().AsQueryable();

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
            var orderedQuery = query.OrderBy(specification.OrderBy);

            foreach (var thenBy in specification.ThenBy)
            {
                orderedQuery = orderedQuery.ThenBy(thenBy);
            }

            foreach (var thenByDesc in specification.ThenByDescending)
            {
                orderedQuery = orderedQuery.ThenByDescending(thenByDesc);
            }

            query = orderedQuery;
        }
        else if (specification.OrderByDescending != null)
        {
            var orderedQuery = query.OrderByDescending(specification.OrderByDescending);

            foreach (var thenBy in specification.ThenBy)
            {
                orderedQuery = orderedQuery.ThenBy(thenBy);
            }

            foreach (var thenByDesc in specification.ThenByDescending)
            {
                orderedQuery = orderedQuery.ThenByDescending(thenByDesc);
            }

            query = orderedQuery;
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
    /// Get entities that match a specification
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get a single entity that matches a specification
    /// </summary>
    public virtual async Task<T?> GetFirstBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Count entities that match a specification
    /// </summary>
    public virtual async Task<int> CountBySpecificationAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<T>().AsQueryable();
        
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }
        
        return await query.CountAsync(cancellationToken);
    }
}
