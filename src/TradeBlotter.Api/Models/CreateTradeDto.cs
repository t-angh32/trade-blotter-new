using System.ComponentModel.DataAnnotations;

namespace TradeBlotter.Api.Models;

public class CreateTradeDto
{
    [Required(ErrorMessage = "Symbol is required.")]
    [MinLength(1, ErrorMessage = "Symbol cannot be empty.")]
    public string Symbol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Side is required.")]
    public TradeSide Side { get; set; }

    [Range(0.000001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Range(0.000001, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}
