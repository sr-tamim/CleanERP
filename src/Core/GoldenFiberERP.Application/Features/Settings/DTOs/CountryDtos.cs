namespace GoldenFiberERP.Application.Features.Settings.DTOs;

/// <summary>
/// Country data transfer object for API responses
/// </summary>
public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Code3 { get; set; } = null;
    public string? NumericCode { get; set; } = null;
    public string? PhoneCode { get; set; } = null;
    public string? Capital { get; set; } = null;
    public string? CurrencyCode { get; set; } = null;
    public string? CurrencySymbol { get; set; } = null;
    public string? TimeZone { get; set; } = null;
    public string? Region { get; set; } = null;
    public string? SubRegion { get; set; } = null;
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}

/// <summary>
/// Simplified country DTO for dropdown lists
/// </summary>
public class CountryLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? PhoneCode { get; set; } = null;
    public string? CurrencyCode { get; set; } = null;
    public string? CurrencySymbol { get; set; } = null;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for creating a new country
/// </summary>
public class CreateCountryDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Code3 { get; set; } = null;
    public string? NumericCode { get; set; } = null;
    public string? PhoneCode { get; set; } = null;
    public string? Capital { get; set; } = null;
    public string? CurrencyCode { get; set; } = null;
    public string? CurrencySymbol { get; set; } = null;
    public string? TimeZone { get; set; } = null;
    public string? Region { get; set; } = null;
    public string? SubRegion { get; set; } = null;
    public int DisplayOrder { get; set; } = 0;
}

/// <summary>
/// DTO for updating an existing country
/// </summary>
public class UpdateCountryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code3 { get; set; } = null;
    public string? NumericCode { get; set; } = null;
    public string? Capital { get; set; } = null;
    public string? CurrencyCode { get; set; } = null;
    public string? CurrencySymbol { get; set; } = null;
    public string? TimeZone { get; set; } = null;
    public string? Region { get; set; } = null;
    public string? SubRegion { get; set; } = null;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
