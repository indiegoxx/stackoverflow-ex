using System.Text.Json;
using StackExchange.Redis;

public interface ICache
{
    Task SetStringAsync(string key, string value, TimeSpan? expiration = null);
    Task<string?> GetStringAsync(string key);
    Task<bool> RemoveAsync(string key);
    Task SetObjectAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> GetObjectAsync<T>(string key);
}



public class RedisCacheService : ICache
{
    private readonly ConnectionMultiplexer _redisConnection;
    private readonly IDatabase _cacheDatabase;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
    {
        _logger = logger;

        // Get the Redis connection string from configuration
        // This will pick up "Redis__ConnectionString" from environment variables in Docker Compose
        // Or "Redis:ConnectionString" from appsettings.json
        string? redisConnectionString = configuration["Redis:ConnectionString"];

        if (string.IsNullOrEmpty(redisConnectionString))
        {
            _logger.LogCritical("Redis connection string is missing. Please configure 'Redis:ConnectionString'.");
            throw new ArgumentNullException(nameof(redisConnectionString), "Redis connection string cannot be null or empty.");
        }

        try
        {
            // Create a single ConnectionMultiplexer instance and reuse it.
            // AbortOnConnectFail = false is often recommended in production to allow background reconnection attempts.
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false;

            _redisConnection = ConnectionMultiplexer.Connect(options);

            // Subscribe to connection events for better diagnostics
            _redisConnection.ConnectionFailed += (sender, e) =>
                _logger.LogError(e.Exception, "Redis connection failed: {FailureType}", e.FailureType);
            _redisConnection.ConnectionRestored += (sender, e) =>
                _logger.LogInformation("Redis connection restored.");
            _redisConnection.ConfigurationChanged += (sender, e) =>
                _logger.LogInformation("Redis configuration changed.");

            _cacheDatabase = _redisConnection.GetDatabase();
            _logger.LogInformation("Successfully connected to Redis: {ConnectionString}", redisConnectionString);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Could not connect to Redis using connection string: {ConnectionString}", redisConnectionString);
            throw; // Re-throw to prevent the application from starting without a critical dependency
        }
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            await _cacheDatabase.StringSetAsync(key, value, expiration);
            _logger.LogDebug("Cache: Set string key '{Key}' with expiration '{Expiration}'.", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache: Failed to set string key '{Key}'.", key);
        }
    }

    public async Task<string?> GetStringAsync(string key)
    {
        try
        {
            var value = await _cacheDatabase.StringGetAsync(key);
            if (value.HasValue)
            {
                _logger.LogDebug("Cache: Got string key '{Key}'.", key);
                return value.ToString();
            }
            _logger.LogDebug("Cache: Key '{Key}' not found.", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache: Failed to get string key '{Key}'.", key);
            return null;
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            var removed = await _cacheDatabase.KeyDeleteAsync(key);
            _logger.LogDebug("Cache: Removed key '{Key}': {Removed}.", key, removed);
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache: Failed to remove key '{Key}'.", key);
            return false;
        }
    }

    public async Task SetObjectAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (value == null)
        {
            _logger.LogWarning("Cache: Attempted to set null object for key '{Key}'.", key);
            return;
        }

        try
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _cacheDatabase.StringSetAsync(key, jsonValue, expiration);
            _logger.LogDebug("Cache: Set object key '{Key}' with expiration '{Expiration}'.", key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache: Failed to set object key '{Key}'.", key);
        }
    }

    public async Task<T?> GetObjectAsync<T>(string key)
    {
        try
        {
            var jsonValue = await _cacheDatabase.StringGetAsync(key);
            if (jsonValue.HasValue)
            {
                _logger.LogDebug("Cache: Got object key '{Key}'.", key);
                return JsonSerializer.Deserialize<T>(jsonValue.ToString());
            }
            _logger.LogDebug("Cache: Object key '{Key}' not found.", key);
            return default(T); // Returns null for reference types, or default value for value types
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache: Failed to get object key '{Key}'.", key);
            return default(T);
        }
    }

    // It's good practice to implement IDisposable if you have unmanaged resources
    // or long-lived connections that need to be closed. ConnectionMultiplexer implements IDisposable.
    // The DI container will call Dispose on singletons when the application shuts down.
    public void Dispose()
    {
        if (_redisConnection != null)
        {
            _redisConnection.Dispose();
            _logger.LogInformation("Redis ConnectionMultiplexer disposed.");
        }
    }
}