using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TradeBlotter.Api.Data;
using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Services;

public class TradePersistenceWorker : BackgroundService
{
    private readonly ITradeQueue _tradeQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TradePersistenceWorker> _logger;

    public TradePersistenceWorker(
        ITradeQueue tradeQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<TradePersistenceWorker> logger)
    {
        _tradeQueue = tradeQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TradePersistenceWorker starting...");

        await foreach (var trade in _tradeQueue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TradeDbContext>();

                await dbContext.Trades.AddAsync(trade, stoppingToken);
                await dbContext.SaveChangesAsync(stoppingToken);

                _logger.LogDebug("Persisted trade ID {TradeId} for symbol {Symbol}", trade.Id, trade.Symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting trade for symbol {Symbol}", trade.Symbol);
            }
        }

        _logger.LogInformation("TradePersistenceWorker stopping.");
    }
}
