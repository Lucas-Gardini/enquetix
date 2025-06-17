using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application.Redis;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.User.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContextPool<Context>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Miscellaneous
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".enquetix.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(1);
});
builder.Services.AddOpenApi();

// Services
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<IMongoDBService, MongoDBService>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// -- User
builder.Services.AddScoped<IUserService, UserService>();

// -- Auth
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandler?.Error is HttpResponseException ex)
        {
            context.Response.StatusCode = ex.Status;
            if (ex.Value != null)
            {
                await context.Response.WriteAsJsonAsync(ex.Value);
            }
        }
    });
});

await app.RunAsync();