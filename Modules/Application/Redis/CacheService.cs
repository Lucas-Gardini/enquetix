using System.Text.Json;
using System.Text.Json.Serialization;

namespace enquetix.Modules.Application.Redis
{
    public class CacheService(IRedisService redis) : ICacheService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or whitespace.", nameof(key));
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            ValidateKey(key);
            var db = await redis.GetDatabaseAsync();
            var value = await db.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!, _jsonOptions) : null;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            ValidateKey(key);
            var db = await redis.GetDatabaseAsync();
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await db.StringSetAsync(key, json, expiry);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            ValidateKey(key);
            var db = await redis.GetDatabaseAsync();
            return await db.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            ValidateKey(key);
            var db = await redis.GetDatabaseAsync();
            return await db.KeyDeleteAsync(key);
        }

        public async Task<T?> CacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class
        {
            ValidateKey(key);
            var value = await GetAsync<T>(key);
            if (value != null)
            {
                return value;
            }
            value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiry);
            }
            return value;
        }
    }

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task<bool> ExistsAsync(string key);
        Task<bool> RemoveAsync(string key);
        Task<T?> CacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class;
    }
}
