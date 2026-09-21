using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Services;

public interface ITradeCacheService
{
    void AddTrade(Trade trade);
    IReadOnlyList<Trade> GetTodayTrades();
    Task SeedAsync(IEnumerable<Trade> initialTrades);
}
