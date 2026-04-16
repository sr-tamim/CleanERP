using CleanERP.Domain.Entities.Settings;
using CleanERP.Domain.Interfaces.Repositories.Common;

namespace CleanERP.Domain.Interfaces.Repositories.Settings;

/// <summary>
/// Repository interface for Country entity operations
/// </summary>
public interface ICountryRepository : IBaseRepository<Country>
{
    /// <summary>
    /// Get country by ISO 2-letter code
    /// </summary>
    Task<Country?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get country by ISO 3-letter code
    /// </summary>
    Task<Country?> GetByCode3Async(string code3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get countries by region
    /// </summary>
    Task<IEnumerable<Country>> GetByRegionAsync(string region, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active countries ordered by display order and name
    /// </summary>
    Task<IEnumerable<Country>> GetActiveCountriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if country code exists (for unique validation)
    /// </summary>
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if country code3 exists (for unique validation)
    /// </summary>
    Task<bool> Code3ExistsAsync(string code3, int? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get countries with pagination and filtering
    /// </summary>
    Task<(IEnumerable<Country> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? region = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}
