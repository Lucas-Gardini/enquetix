using MongoDB.Driver;

namespace enquetix.Modules.Application
{
    public class MongoDBService : IMongoDBService
    {
        private readonly MongoClient _client;
        private readonly string _databaseName;

        public MongoDBService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoDB");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("MongoDB connection string is missing.");

            var uri = new Uri(connectionString!);

            _databaseName = uri.AbsolutePath.TrimStart('/');
            _client = new MongoClient(connectionString);
        }

        private IMongoCollection<T> GetCollection<T>() where T : class
        {
            var database = _client.GetDatabase(_databaseName);
            return database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
        }

        public async Task<T> FirstOrDefaultAsync<T>(FilterDefinition<T> filter) where T : class
        {
            var collection = GetCollection<T>();
            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<T>> FindAsync<T>(FilterDefinition<T> filter) where T : class
        {
            var collection = GetCollection<T>();
            return await collection.Find(filter).ToListAsync();
        }

        public async Task InsertAsync<T>(T document) where T : class
        {
            var collection = GetCollection<T>();
            await collection.InsertOneAsync(document);
        }

        public async Task UpdateAsync<T>(FilterDefinition<T> filter, UpdateDefinition<T> update) where T : class
        {
            var collection = GetCollection<T>();
            await collection.UpdateOneAsync(filter, update);
        }

        public async Task DeleteAsync<T>(FilterDefinition<T> filter) where T : class
        {
            var collection = GetCollection<T>();
            await collection.DeleteOneAsync(filter);
        }

        public async Task<bool> ExistsAsync<T>(FilterDefinition<T> filter) where T : class
        {
            var collection = GetCollection<T>();
            return await collection.Find(filter).AnyAsync();
        }

        public async Task<long> CountAsync<T>(FilterDefinition<T> filter) where T : class
        {
            var collection = GetCollection<T>();
            return await collection.CountDocumentsAsync(filter);
        }
    }

    public interface IMongoDBService
    {
        Task<T> FirstOrDefaultAsync<T>(FilterDefinition<T> filter) where T : class;
        Task<List<T>> FindAsync<T>(FilterDefinition<T> filter) where T : class;
        Task InsertAsync<T>(T document) where T : class;
        Task UpdateAsync<T>(FilterDefinition<T> filter, UpdateDefinition<T> update) where T : class;
        Task DeleteAsync<T>(FilterDefinition<T> filter) where T : class;
        Task<bool> ExistsAsync<T>(FilterDefinition<T> filter) where T : class;
        Task<long> CountAsync<T>(FilterDefinition<T> filter) where T : class;
    }
}
