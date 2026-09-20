namespace TradeBlotter.Api.Services;

public interface ITradeIdGenerator
{
    void Initialize(int maxId);
    int GetNextId();
}
