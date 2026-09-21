using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TradeBlotter.Api.Hubs;
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
    private readonly ITradeIdGenerator _idGenerator;
    private readonly IHubContext<TradeHub> _hubContext;

    public TradesController(
        ITradeCacheService tradeCache,
        ITradeQueue tradeQueue,
        IPositionCalculatorService positionCalculator,
        ITradeIdGenerator idGenerator,
        IHubContext<TradeHub> hubContext)
    {
        _tradeCache = tradeCache;
        _tradeQueue = tradeQueue;
        _positionCalculator = positionCalculator;
        _idGenerator = idGenerator;
        _hubContext = hubContext;
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
            Id = _idGenerator.GetNextId(),
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

        // 3. Broadcast real-time SignalR notification to all connected blotter clients
        await _hubContext.Clients.All.SendAsync("TradeExecuted", trade);

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
