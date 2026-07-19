using System.Text.Json;
using CleanERP.Application.Common.Interfaces;

namespace CleanERP.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private static readonly Dictionary<string, CacheItem> _cache = new();
    private static readonly object _lock = new();

    public async Task<T?> GetAsync<T>(string key)
    {
        await Task.CompletedTask;

        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.ExpiresAt == null || item.ExpiresAt > DateTime.UtcNow)
                {
                    return JsonSerializer.Deserialize<T>(item.Value);
                }
                else
                {
                    _cache.Remove(key);
                }
            }
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        await Task.CompletedTask;

        var json = JsonSerializer.Serialize(value);
        DateTime? expiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null;

        lock (_lock)
        {
            _cache[key] = new CacheItem(json, expiresAt);
        }
    }

    public async Task RemoveAsync(string key)
    {
        await Task.CompletedTask;

        lock (_lock)
        {
            _cache.Remove(key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await Task.CompletedTask;

        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.ExpiresAt == null || item.ExpiresAt > DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    _cache.Remove(key);
                }
            }
        }

        return false;
    }

    private record CacheItem(string Value, DateTime? ExpiresAt);
}
