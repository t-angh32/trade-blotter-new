namespace TradeBlotter.Api.Services;

public class TradeIdGenerator : ITradeIdGenerator
{
    private int _currentId = 0;
    private bool _initialized = false;
    private readonly object _lock = new();

    public void Initialize(int maxId)
    {
        lock (_lock)
        {
            if (!_initialized || maxId > _currentId)
            {
                _currentId = maxId;
                _initialized = true;
            }
        }
    }

    public int GetNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }
}
