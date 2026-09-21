using System;
using System.Collections.Generic;
using TradeBlotter.Api.Models;
using TradeBlotter.Api.Services;
using Xunit;

namespace TradeBlotter.Tests;

public class PositionCalculatorServiceTests
{
    private readonly PositionCalculatorService _calculator = new();

    [Fact]
    public void CalculatePositions_SingleBuy_ReturnsCorrectPosition()
    {
        // Arrange
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "AAPL", Side = TradeSide.Buy, Quantity = 100m, Price = 150.00m, Timestamp = DateTime.UtcNow }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Single(positions);
        Assert.Equal("AAPL", positions[0].Symbol);
        Assert.Equal(100m, positions[0].NetQuantity);
        Assert.Equal(150.00m, positions[0].AverageCost);
    }

    [Fact]
    public void CalculatePositions_MultipleBuys_CalculatesWeightedAverageCost()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "AAPL", Side = TradeSide.Buy, Quantity = 100m, Price = 100.00m, Timestamp = now },
            new Trade { Id = 2, Symbol = "AAPL", Side = TradeSide.Buy, Quantity = 100m, Price = 200.00m, Timestamp = now.AddMinutes(1) }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Single(positions);
        Assert.Equal("AAPL", positions[0].Symbol);
        Assert.Equal(200m, positions[0].NetQuantity);
        Assert.Equal(150.00m, positions[0].AverageCost);
    }

    [Fact]
    public void CalculatePositions_MixedBuysAndSells_RetainsAvgCostOnSell()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "AAPL", Side = TradeSide.Buy, Quantity = 100m, Price = 100.00m, Timestamp = now },
            new Trade { Id = 2, Symbol = "AAPL", Side = TradeSide.Buy, Quantity = 100m, Price = 200.00m, Timestamp = now.AddMinutes(1) },
            new Trade { Id = 3, Symbol = "AAPL", Side = TradeSide.Sell, Quantity = 50m, Price = 180.00m, Timestamp = now.AddMinutes(2) }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Single(positions);
        Assert.Equal("AAPL", positions[0].Symbol);
        Assert.Equal(150m, positions[0].NetQuantity);
        Assert.Equal(150.00m, positions[0].AverageCost);
    }

    [Fact]
    public void CalculatePositions_NetZeroPosition_OmitsSymbolFromResponse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "MSFT", Side = TradeSide.Buy, Quantity = 100m, Price = 300.00m, Timestamp = now },
            new Trade { Id = 2, Symbol = "MSFT", Side = TradeSide.Sell, Quantity = 100m, Price = 310.00m, Timestamp = now.AddMinutes(1) }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Empty(positions);
    }

    [Fact]
    public void CalculatePositions_ShortPosition_CalculatesShortNetQtyAndAverageShortPrice()
    {
        // Arrange
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "TSLA", Side = TradeSide.Sell, Quantity = 100m, Price = 200.00m, Timestamp = DateTime.UtcNow }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Single(positions);
        Assert.Equal("TSLA", positions[0].Symbol);
        Assert.Equal(-100m, positions[0].NetQuantity);
        Assert.Equal(200.00m, positions[0].AverageCost);
    }

    [Fact]
    public void CalculatePositions_ShortPositionCover_RetainsAvgShortCostOnPartialCover()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "TSLA", Side = TradeSide.Sell, Quantity = 100m, Price = 200.00m, Timestamp = now },
            new Trade { Id = 2, Symbol = "TSLA", Side = TradeSide.Buy, Quantity = 40m, Price = 180.00m, Timestamp = now.AddMinutes(1) }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Single(positions);
        Assert.Equal("TSLA", positions[0].Symbol);
        Assert.Equal(-60m, positions[0].NetQuantity);
        Assert.Equal(200.00m, positions[0].AverageCost);
    }

    [Fact]
    public void CalculatePositions_FlipFromShortToLong_UpdatesAvgCostToNewLongPrice()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var trades = new List<Trade>
        {
            new Trade { Id = 1, Symbol = "TSLA", Side = TradeSide.Sell, Quantity = 100m, Price = 200.00m, Timestamp = now },
            new Trade { Id = 2, Symbol = "TSLA", Side = TradeSide.Buy, Quantity = 150m, Price = 220.00m, Timestamp = now.AddMinutes(1) }
        };

        // Act
        var positions = _calculator.CalculatePositions(trades);

        // Assert
        Assert.Single(positions);
        Assert.Equal("TSLA", positions[0].Symbol);
        Assert.Equal(50m, positions[0].NetQuantity);
        Assert.Equal(220.00m, positions[0].AverageCost);
    }
}
