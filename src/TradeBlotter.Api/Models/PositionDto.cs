namespace TradeBlotter.Api.Models;

public class PositionDto
{
    public string Symbol { get; set; } = string.Empty;
    public decimal NetQuantity { get; set; }
    public decimal AverageCost { get; set; }
}
