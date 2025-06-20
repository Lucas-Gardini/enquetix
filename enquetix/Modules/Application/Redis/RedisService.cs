using StackExchange.Redis;

namespace enquetix.Modules.Application.Redis
{
    public class RedisService : IRedisService, IAsyncDisposable
    {
        private readonly Lazy<Task<ConnectionMultiplexer>> _lazyConnection;
        private bool _disposed;

        public RedisService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Redis");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Redis connection string is missing.");
            }

            _lazyConnection = new Lazy<Task<ConnectionMultiplexer>>(async () =>
            {
                return await ConnectionMultiplexer.ConnectAsync(connectionString);
            });
        }

        private async Task<ConnectionMultiplexer> GetConnectionAsync()
        {
            return await _lazyConnection.Value;
        }

        public async Task<IDatabase> GetDatabaseAsync()
        {
            var connection = await GetConnectionAsync();
            return connection.GetDatabase();
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_lazyConnection.IsValueCreated)
                {
                    var connection = await _lazyConnection.Value;
                    if (connection.IsConnected)
                    {
                        await connection.CloseAsync();
                    }
                    await connection.DisposeAsync();
                }
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        ~RedisService()
        {
            if (!_disposed)
            {
                DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }
    }

    public interface IRedisService
    {
        Task<IDatabase> GetDatabaseAsync();
    }
}
