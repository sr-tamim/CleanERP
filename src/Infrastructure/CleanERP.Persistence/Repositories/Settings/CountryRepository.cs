using Microsoft.EntityFrameworkCore;
using CleanERP.Domain.Entities.Settings;
using CleanERP.Domain.Interfaces.Repositories.Settings;
using CleanERP.Domain.Specifications;
using CleanERP.Persistence.Repositories.Common;
using CleanERP.Persistence.Contexts;

namespace CleanERP.Persistence.Repositories.Settings;

/// <summary>
/// Repository implementation for Country entity
/// </summary>
public class CountryRepository : BaseRepository<Country>, ICountryRepository
{
    public CountryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Country?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Country>()
            .FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<Country?> GetByCode3Async(string code3, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Country>()
            .FirstOrDefaultAsync(c => c.Code3 == code3.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<Country>> GetByRegionAsync(string region, CancellationToken cancellationToken = default)
    {
        return await _context.Set<Country>()
            .Where(c => c.Region == region)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Country>> GetActiveCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<Country>()
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Country>()
            .Where(c => c.Code == code.ToUpperInvariant());

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> Code3ExistsAsync(string code3, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Country>()
            .Where(c => c.Code3 == code3.ToUpperInvariant());

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Country> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? region = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Country>().AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            query = query.Where(c => 
                c.Name.ToLower().Contains(lowerSearchTerm) ||
                c.Code.ToLower().Contains(lowerSearchTerm) ||
                (c.Code3 != null && c.Code3.ToLower().Contains(lowerSearchTerm)) ||
                (c.Capital != null && c.Capital.ToLower().Contains(lowerSearchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(region))
        {
            query = query.Where(c => c.Region == region);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Example: Get countries using specifications
    /// </summary>
    public async Task<IEnumerable<Country>> GetCountriesWithSpecificationAsync(ISpecification<Country> specification, CancellationToken cancellationToken = default)
    {
        return await GetBySpecificationAsync(specification, cancellationToken);
    }

    /// <summary>
    /// Example: Get active countries using specification
    /// </summary>
    public async Task<IEnumerable<Country>> GetActiveCountriesWithSpecAsync(CancellationToken cancellationToken = default)
    {
        var specification = new Domain.Specifications.Settings.ActiveCountriesSpecification();
        return await GetBySpecificationAsync(specification, cancellationToken);
    }
}
