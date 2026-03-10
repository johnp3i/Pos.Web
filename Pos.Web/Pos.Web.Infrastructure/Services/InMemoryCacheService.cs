using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// In-memory cache service fallback when Redis is not available
/// Used for development environments without Redis installed
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    public InMemoryCacheService(ILogger logger)
    {
        _cache = new MemoryCache(new MemoryCacheOptions
        {
            SizeLimit = 1024 // Limit to 1024 entries
        });
        _logger = logger;
        _logger.LogInformation("InMemoryCacheService initialized - using in-memory cache fallback");
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return value;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            _cache.Set(key, value, cacheEntryOptions);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return _cache.TryGetValue(key, out _);
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        try
        {
            if (_cache.TryGetValue(key, out T? cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue!;
            }

            _logger.LogDebug("Cache miss for key: {Key}, executing factory", key);
            var value = await factory();

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };

            _cache.Set(key, value, cacheEntryOptions);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            return await factory();
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        _logger.LogWarning("RemoveByPatternAsync not fully supported in InMemoryCache - pattern: {Pattern}", pattern);
        // In-memory cache doesn't support pattern-based removal efficiently
        // This is a limitation of the fallback implementation
    }
}
