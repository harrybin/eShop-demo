namespace eShop.Ordering.API.Application.Queries;

public class OrderQueries(OrderingContext context)
    : IOrderQueries
{
    public async Task<Order> GetOrderAsync(int id)
    {
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            throw new KeyNotFoundException();

        return new Order
        {
            OrderNumber = order.Id,
            Date = order.OrderDate,
            Description = order.Description,
            City = order.Address.City,
            Country = order.Address.Country,
            State = order.Address.State,
            Street = order.Address.Street,
            Zipcode = order.Address.ZipCode,
            Status = order.OrderStatus.ToString(),
            Total = order.GetTotal(),
            OrderItems = order.OrderItems.Select(oi => new Orderitem
            {
                ProductName = oi.ProductName,
                Units = oi.Units,
                UnitPrice = (double)oi.UnitPrice,
                PictureUrl = oi.PictureUrl
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId)
    {
        return await GetOrdersFromUserAsync(userId, new OrderHistoryQuery());
    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId, OrderHistoryQuery query)
    {
        query ??= new OrderHistoryQuery();

        var pageNumber = Math.Max(1, query.PageNumber ?? 1);
        var pageSize = Math.Clamp(query.PageSize ?? 20, 1, 100);

        var ordersQuery = context.Orders.Where(o => o.Buyer.IdentityGuid == userId);

        if (query.Status.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderStatus == query.Status.Value);
        }

        if (query.FromDateUtc.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= query.FromDateUtc.Value);
        }

        if (query.ToDateUtc.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= query.ToDateUtc.Value);
        }

        return await ordersQuery
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummary
            {
                OrderNumber = o.Id,
                Date = o.OrderDate,
                Status = o.OrderStatus.ToString(),
                Total = (double)o.OrderItems.Sum(oi => oi.UnitPrice * oi.Units)
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<CardType>> GetCardTypesAsync() =>
        await context.CardTypes.Select(c => new CardType { Id = c.Id, Name = c.Name }).ToListAsync();
}
