namespace CleanERP.Shared.Configuration;

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool SaveToken { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Cookie configuration settings
/// </summary>
public class CookieSettings
{
    public const string SectionName = "CookieSettings";
    
    public string AccessTokenCookieName { get; set; } = "CleanERP.AccessToken";
    public string RefreshTokenCookieName { get; set; } = "CleanERP.RefreshToken";
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
    public string Domain { get; set; } = string.Empty;
    public string Path { get; set; } = "/";
    public bool HttpOnly { get; set; } = true;
    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
}

/// <summary>
/// Password policy configuration settings
/// </summary>
public class PasswordSettings
{
    public const string SectionName = "PasswordSettings";
    
    public int RequiredLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
}

/// <summary>
/// Security configuration settings
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";
    
    public bool EnableTwoFactorAuthentication { get; set; } = false;
    public int MaxLoginAttempts { get; set; } = 5;
    public TimeSpan AccountLockoutDuration { get; set; } = TimeSpan.FromMinutes(30);
    public bool RequireUniqueEmail { get; set; } = true;
    public TimeSpan AllowedPasswordResetTokenLifetime { get; set; } = TimeSpan.FromHours(1);
}
