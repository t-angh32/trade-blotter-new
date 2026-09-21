using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Services;

public interface IPositionCalculatorService
{
    IReadOnlyList<PositionDto> CalculatePositions(IEnumerable<Trade> trades);
}
