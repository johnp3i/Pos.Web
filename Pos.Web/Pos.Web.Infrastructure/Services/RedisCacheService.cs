using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Pos.Web.Infrastructure.Services;

/// <summary>
/// Redis-based cache service implementation
/// Provides distributed caching with expiration policies and key prefix management
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly string _keyPrefix;
    private readonly TimeSpan _defaultExpiration;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger,
        string keyPrefix = "pos:",
        TimeSpan? defaultExpiration = null)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _database = _redis.GetDatabase();
        _keyPrefix = keyPrefix;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Get a cached value by key
    /// </summary>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            var value = await _database.StringGetAsync(prefixedKey);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Set a cached value with optional expiration
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            var serializedValue = JsonSerializer.Serialize(value);
            var expirationTime = expiration ?? _defaultExpiration;

            await _database.StringSetAsync(prefixedKey, serializedValue, expirationTime);
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Remove a cached value by key
    /// </summary>
    public async Task RemoveAsync(string key)
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            await _database.KeyDeleteAsync(prefixedKey);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Check if a key exists in cache
    /// </summary>
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var prefixedKey = GetPrefixedKey(key);
            return await _database.KeyExistsAsync(prefixedKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Remove all cached values matching a pattern
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var prefixedPattern = GetPrefixedKey(pattern);
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: prefixedPattern).ToArray();

            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug("Removed {Count} cached values matching pattern: {Pattern}", keys.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached values by pattern: {Pattern}", pattern);
            throw;
        }
    }

    /// <summary>
    /// Get or create a cached value using a factory function
    /// </summary>
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        try
        {
            // Try to get from cache first
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Cache miss - create value using factory
            _logger.LogDebug("Cache miss for key: {Key}, creating value using factory", key);
            var value = await factory();

            // Cache the newly created value
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Get prefixed key to avoid collisions with other applications
    /// </summary>
    private string GetPrefixedKey(string key)
    {
        return $"{_keyPrefix}{key}";
    }
}
