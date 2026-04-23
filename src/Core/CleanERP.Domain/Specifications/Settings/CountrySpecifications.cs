using CleanERP.Domain.Entities.Settings;
using CleanERP.Domain.Specifications;

namespace CleanERP.Domain.Specifications.Settings;

/// <summary>
/// Specification for active countries
/// </summary>
public class ActiveCountriesSpecification : BaseSpecification<Country>
{
    public ActiveCountriesSpecification() 
        : base(c => c.IsActive)
    {
        AddOrderBy(c => c.DisplayOrder);
        AddThenBy(c => c.Name);
    }
}

/// <summary>
/// Specification for countries by region
/// </summary>
public class CountriesByRegionSpecification : BaseSpecification<Country>
{
    public CountriesByRegionSpecification(string region) 
        : base(c => c.Region == region)
    {
        AddOrderBy(c => c.DisplayOrder);
        AddThenBy(c => c.Name);
    }
}

/// <summary>
/// Specification for countries with search term
/// </summary>
public class CountriesWithSearchSpecification : BaseSpecification<Country>
{
    public CountriesWithSearchSpecification(string searchTerm) 
        : base(c => c.Name.Contains(searchTerm) ||
                   c.Code.Contains(searchTerm) ||
                   (c.Code3 != null && c.Code3.Contains(searchTerm)) ||
                   (c.Capital != null && c.Capital.Contains(searchTerm)))
    {
        AddOrderBy(c => c.Name);
    }
}

/// <summary>
/// Specification for countries with pagination and filtering
/// </summary>
public class CountriesWithFiltersSpecification : BaseSpecification<Country>
{
    public CountriesWithFiltersSpecification(
        string? searchTerm = null,
        string? region = null,
        bool? isActive = null,
        int? skip = null,
        int? take = null)
        : base(BuildCriteria(searchTerm, region, isActive))
    {
        AddOrderBy(c => c.DisplayOrder);
        AddThenBy(c => c.Name);
        if (skip.HasValue && take.HasValue)
        {
            ApplyPaging(skip.Value, take.Value);
        }
    }

    private static System.Linq.Expressions.Expression<Func<Country, bool>>? BuildCriteria(
        string? searchTerm, 
        string? region, 
        bool? isActive)
    {
        return c => (isActive == null || c.IsActive == isActive) &&
                   (string.IsNullOrEmpty(region) || c.Region == region) &&
                   (string.IsNullOrEmpty(searchTerm) || 
                    c.Name.Contains(searchTerm) ||
                    c.Code.Contains(searchTerm) ||
                    (c.Code3 != null && c.Code3.Contains(searchTerm)) ||
                    (c.Capital != null && c.Capital.Contains(searchTerm)));
    }
}

/// <summary>
/// Specification for countries by code
/// </summary>
public class CountryByCodeSpecification : BaseSpecification<Country>
{
    public CountryByCodeSpecification(string code) 
        : base(c => c.Code == code.ToUpperInvariant())
    {
    }
}

/// <summary>
/// Specification for countries by code3
/// </summary>
public class CountryByCode3Specification : BaseSpecification<Country>
{
    public CountryByCode3Specification(string code3) 
        : base(c => c.Code3 != null && c.Code3 == code3.ToUpperInvariant())
    {
    }
}
