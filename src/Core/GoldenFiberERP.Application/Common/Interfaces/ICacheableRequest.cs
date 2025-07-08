namespace GoldenFiberERP.Application.Common.Interfaces;

/// <summary>
/// Interface for requests that support caching
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// Cache key for this request
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Cache expiration time
    /// </summary>
    TimeSpan CacheExpiration { get; }
}
