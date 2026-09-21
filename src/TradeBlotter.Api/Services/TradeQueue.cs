using System.Threading.Channels;
using TradeBlotter.Api.Models;

namespace TradeBlotter.Api.Services;

public interface ITradeQueue
{
    ValueTask QueueTradeAsync(Trade trade, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Trade> ReadAllAsync(CancellationToken cancellationToken = default);
}

public class TradeQueue : ITradeQueue
{
    private readonly Channel<Trade> _channel;

    public TradeQueue(int capacity = 10000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<Trade>(options);
    }

    public ValueTask QueueTradeAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(trade, cancellationToken);
    }

    public IAsyncEnumerable<Trade> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
