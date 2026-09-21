using Microsoft.AspNetCore.Mvc;
using TradeBlotter.Api.Models;
using TradeBlotter.Api.Services;

namespace TradeBlotter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TradesController : ControllerBase
{
    private readonly ITradeCacheService _tradeCache;
    private readonly ITradeQueue _tradeQueue;
    private readonly IPositionCalculatorService _positionCalculator;
    private static int _tradeIdCounter = 0;

    public TradesController(
        ITradeCacheService tradeCache,
        ITradeQueue tradeQueue,
        IPositionCalculatorService positionCalculator)
    {
        _tradeCache = tradeCache;
        _tradeQueue = tradeQueue;
        _positionCalculator = positionCalculator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTrade([FromBody] CreateTradeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var trade = new Trade
        {
            Id = Interlocked.Increment(ref _tradeIdCounter),
            Symbol = dto.Symbol.Trim().ToUpperInvariant(),
            Side = dto.Side,
            Quantity = Math.Round(dto.Quantity, 4),
            Price = Math.Round(dto.Price, 4),
            Timestamp = DateTime.UtcNow
        };

        // 1. Instantly update in-memory trade cache
        _tradeCache.AddTrade(trade);

        // 2. Enqueue non-blockingly for background SQLite DB persistence
        await _tradeQueue.QueueTradeAsync(trade);

        return CreatedAtAction(nameof(GetTrades), new { id = trade.Id }, trade);
    }

    [HttpGet]
    public IActionResult GetTrades()
    {
        var trades = _tradeCache.GetTodayTrades();
        return Ok(trades);
    }

    [HttpGet("/positions")]
    public IActionResult GetPositions()
    {
        var todayTrades = _tradeCache.GetTodayTrades();
        var positions = _positionCalculator.CalculatePositions(todayTrades);
        return Ok(positions);
    }
}
