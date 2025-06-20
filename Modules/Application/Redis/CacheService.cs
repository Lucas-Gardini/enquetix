using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace enquetix.Modules.Application.Redis
{
    public class CacheService : ICacheService
    {
        private readonly IRedisService _redis;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public CacheService(IRedisService redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis), "Redis service cannot be null.");
            ClearCache().GetAwaiter().GetResult();
        }

        private static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key cannot be null or whitespace.", nameof(key));
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            ValidateKey(key);
            var db = await _redis.GetDatabaseAsync();
            var value = await db.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!, _jsonOptions) : null;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            ValidateKey(key);
            var db = await _redis.GetDatabaseAsync();
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await db.StringSetAsync(key, json, expiry);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            ValidateKey(key);
            var db = await _redis.GetDatabaseAsync();
            return await db.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            ValidateKey(key);
            var db = await _redis.GetDatabaseAsync();
            return await db.KeyDeleteAsync(key);
        }

        public async Task<bool> RemoveAsync(string[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentException("Keys cannot be null or empty.", nameof(keys));
            var db = await _redis.GetDatabaseAsync();
            var redisKeys = keys.Select(k => (RedisKey)k).ToArray();
            return await db.KeyDeleteAsync(redisKeys) > 0;
        }

        public async Task<bool> RemoveByPartialNameAsync(string partialName)
        {
            if (string.IsNullOrWhiteSpace(partialName))
                throw new ArgumentException("Partial name cannot be null or whitespace.", nameof(partialName));
            var db = await _redis.GetDatabaseAsync();
            var result = await db.ExecuteAsync("KEYS", $"*{partialName}*");
            if (result.IsNull)
            {
                return false;
            }
            var redisValues = (RedisResult[])result!;
            var keys = redisValues.Select(rv => (RedisKey)(string)rv!).ToArray();
            if (keys.Length > 0)
            {
                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
                return true;
            }
            return false;
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

        private async Task ClearCache()
        {
            var db = await _redis.GetDatabaseAsync();

            var result = await db.ExecuteAsync("KEYS", "*");

            if (result.IsNull)
            {
                return;
            }

            var redisValues = (RedisResult[])result!;
            var keys = redisValues.Select(rv => (RedisKey)(string)rv!).Where(s => !s.ToString().StartsWith("session:")).ToArray();

            if (keys.Length > 0)
            {
                foreach (var key in keys)
                {
                    await db.KeyDeleteAsync(key);
                }
            }
        }
    }

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
        Task<bool> ExistsAsync(string key);
        Task<bool> RemoveAsync(string key);
        Task<bool> RemoveAsync(string[] keys);
        Task<bool> RemoveByPartialNameAsync(string partialName);
        Task<T?> CacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class;
    }
}
