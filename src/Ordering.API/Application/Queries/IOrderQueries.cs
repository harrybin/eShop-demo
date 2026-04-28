namespace eShop.Ordering.API.Application.Queries;

public interface IOrderQueries
{
    Task<Order> GetOrderAsync(int id, CancellationToken cancellationToken);

    Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId, CancellationToken cancellationToken);

    Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId, OrderHistoryQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<CardType>> GetCardTypesAsync(CancellationToken cancellationToken);
}
