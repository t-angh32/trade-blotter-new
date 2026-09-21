using System.Collections.Concurrent;
using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Services;

public class TradeCacheService : ITradeCacheService
{
    private readonly ConcurrentBag<Trade> _trades = new();

    public void AddTrade(Trade trade)
    {
        _trades.Add(trade);
    }

    public IReadOnlyList<Trade> GetTodayTrades()
    {
        var todayUtc = DateTime.UtcNow.Date;
        return _trades
            .Where(t => t.Timestamp.Date == todayUtc)
            .OrderByDescending(t => t.Timestamp)
            .ToList();
    }

    public Task SeedAsync(IEnumerable<Trade> initialTrades)
    {
        foreach (var trade in initialTrades)
        {
            _trades.Add(trade);
        }
        return Task.CompletedTask;
    }
}
