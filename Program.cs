using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application.Redis;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContextPool<Context>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));

// Miscellaneous
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Services
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IMongoDBService, MongoDBService>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

await app.RunAsync();