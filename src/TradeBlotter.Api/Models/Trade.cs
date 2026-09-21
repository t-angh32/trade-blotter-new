namespace TradeBlotter.Api.Models;

public class Trade
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}
