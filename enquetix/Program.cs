using enquetix.Modules.Application;
using enquetix.Modules.Application.EntityFramework;
using enquetix.Modules.Application.Redis;
using enquetix.Modules.Auth.Services;
using enquetix.Modules.Poll.Services;
using enquetix.Modules.User.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Database & Services
builder.Services.AddDbContextPool<Context>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql")));
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "session:";
});
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<IMongoDBService, MongoDBService>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

// Miscellaneous
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".enquetix.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromDays(1);
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// Services
// -- Cache
builder.Services.AddSingleton<ICacheService, CacheService>();

// -- User
builder.Services.AddScoped<IUserService, UserService>();

// -- Auth
builder.Services.AddScoped<IAuthService, AuthService>();

// -- Poll
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IPollOptionService, PollOptionService>();
builder.Services.AddScoped<IPollVoteService, PollVoteService>();
builder.Services.AddSingleton<IPollVoteQueueManager, PollVoteQueueManager>();

var app = builder.Build();
app.Services.GetRequiredService<IPollVoteQueueManager>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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

app.UseSession();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();