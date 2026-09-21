using Microsoft.EntityFrameworkCore;
using TradeBlotter.Api.Data;
using TradeBlotter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


// SQLite Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=tradeblotter.db";
builder.Services.AddDbContext<TradeDbContext>(options =>
    options.UseSqlite(connectionString));

// Singletons & Services
builder.Services.AddSingleton<ITradeCacheService, TradeCacheService>();
builder.Services.AddSingleton<ITradeQueue, TradeQueue>();
builder.Services.AddSingleton<IPositionCalculatorService, PositionCalculatorService>();
builder.Services.AddHostedService<TradePersistenceWorker>();

// CORS Policy for Vue Vite Frontend (default http://localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 2. Initialize Database & Seed In-Memory Cache on Startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TradeDbContext>();
    var cacheService = scope.ServiceProvider.GetRequiredService<ITradeCacheService>();

    await dbContext.Database.EnsureCreatedAsync();

    var todayUtc = DateTime.UtcNow.Date;
    var existingTodayTrades = await dbContext.Trades
        .Where(t => t.Timestamp >= todayUtc)
        .ToListAsync();

    await cacheService.SeedAsync(existingTodayTrades);
}

// 3. Configure HTTP Pipeline
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
