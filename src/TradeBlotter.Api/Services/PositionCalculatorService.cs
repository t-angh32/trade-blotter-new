using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Services;

public class PositionCalculatorService : IPositionCalculatorService
{
    public IReadOnlyList<PositionDto> CalculatePositions(IEnumerable<Trade> trades)
    {
        var result = new List<PositionDto>();

        var groupedTrades = trades
            .Where(t => !string.IsNullOrWhiteSpace(t.Symbol))
            .GroupBy(t => t.Symbol.Trim().ToUpperInvariant());

        foreach (var group in groupedTrades)
        {
            decimal netQty = 0m;
            decimal avgCost = 0m;

            var orderedTrades = group.OrderBy(t => t.Timestamp).ThenBy(t => t.Id);

            foreach (var trade in orderedTrades)
            {
                if (trade.Side == TradeSide.Buy)
                {
                    if (netQty >= 0m)
                    {
                        // Expanding or opening a long position
                        decimal newQty = netQty + trade.Quantity;
                        avgCost = newQty > 0m
                            ? ((netQty * avgCost) + (trade.Quantity * trade.Price)) / newQty
                            : 0m;
                        netQty = newQty;
                    }
                    else
                    {
                        // Covering a short position (netQty is negative)
                        decimal absShortQty = Math.Abs(netQty);
                        if (trade.Quantity < absShortQty)
                        {
                            // Partial cover
                            netQty += trade.Quantity;
                        }
                        else if (trade.Quantity == absShortQty)
                        {
                            // Full cover -> position closed
                            netQty = 0m;
                            avgCost = 0m;
                        }
                        else
                        {
                            // Over-cover -> flipped to long position
                            decimal excessLongQty = trade.Quantity - absShortQty;
                            netQty = excessLongQty;
                            avgCost = trade.Price;
                        }
                    }
                }
                else if (trade.Side == TradeSide.Sell)
                {
                    if (netQty > 0m)
                    {
                        // Reducing or closing a long position
                        if (trade.Quantity < netQty)
                        {
                            // Partial sell
                            netQty -= trade.Quantity;
                        }
                        else if (trade.Quantity == netQty)
                        {
                            // Full sell -> position closed
                            netQty = 0m;
                            avgCost = 0m;
                        }
                        else
                        {
                            // Over-sell -> flipped to short position
                            decimal excessShortQty = trade.Quantity - netQty;
                            netQty = -excessShortQty;
                            avgCost = trade.Price;
                        }
                    }
                    else
                    {
                        // Expanding or opening a short position (netQty <= 0)
                        decimal absShortQty = Math.Abs(netQty);
                        decimal newAbsShortQty = absShortQty + trade.Quantity;
                        avgCost = newAbsShortQty > 0m
                            ? ((absShortQty * avgCost) + (trade.Quantity * trade.Price)) / newAbsShortQty
                            : 0m;
                        netQty = -newAbsShortQty;
                    }
                }
            }

            // Omit symbols with a net position of zero
            if (Math.Abs(netQty) >= 0.000001m)
            {
                result.Add(new PositionDto
                {
                    Symbol = group.Key,
                    NetQuantity = Math.Round(netQty, 4),
                    AverageCost = Math.Round(avgCost, 4)
                });
            }
        }

        return result.OrderBy(p => p.Symbol).ToList();
    }
}
