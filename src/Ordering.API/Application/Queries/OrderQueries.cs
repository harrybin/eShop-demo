namespace eShop.Ordering.API.Application.Queries;

public class OrderQueries(OrderingContext context)
    : IOrderQueries
{
    private static readonly (OrderStatus Status, string Label)[] TimelineSequence =
    [
        (OrderStatus.Submitted, "Order submitted"),
        (OrderStatus.AwaitingValidation, "Awaiting validation"),
        (OrderStatus.StockConfirmed, "Stock confirmed"),
        (OrderStatus.Paid, "Payment confirmed"),
        (OrderStatus.Shipped, "Order shipped")
    ];

    public async Task<Order> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

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
            Timeline = BuildTimeline(order),
            OrderItems = order.OrderItems.Select(oi => new Orderitem
            {
                ProductName = oi.ProductName,
                Units = oi.Units,
                UnitPrice = (double)oi.UnitPrice,
                PictureUrl = oi.PictureUrl
            }).ToList()
        };
    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await GetOrdersFromUserAsync(userId, new OrderHistoryQuery(), cancellationToken);
    }

    public async Task<IEnumerable<OrderSummary>> GetOrdersFromUserAsync(string userId, OrderHistoryQuery query, CancellationToken cancellationToken)
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
            .AsNoTracking()
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
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CardType>> GetCardTypesAsync(CancellationToken cancellationToken) =>
        await context.CardTypes
            .AsNoTracking()
            .Select(c => new CardType { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);

    private static List<OrderTimelineEntry> BuildTimeline(eShop.Ordering.Domain.AggregatesModel.OrderAggregate.Order order)
    {
        var actualEntries = order.StatusHistory
            .OrderBy(entry => entry.StatusChangedOnUtc)
            .GroupBy(entry => entry.Status)
            .ToDictionary(group => group.Key, group => group.First());

        if (actualEntries.Count == 0)
        {
            actualEntries = BuildFallbackHistory(order);
        }

        var cancelledEntry = actualEntries.GetValueOrDefault(OrderStatus.Cancelled);
        var timeline = new List<OrderTimelineEntry>();

        foreach (var (status, label) in TimelineSequence)
        {
            var entry = actualEntries.GetValueOrDefault(status);
            if (entry is not null)
            {
                timeline.Add(new OrderTimelineEntry
                {
                    Status = status.ToString(),
                    Label = label,
                    State = "completed",
                    Timestamp = entry.StatusChangedOnUtc,
                    Detail = entry.Reason
                });
                continue;
            }

            if (cancelledEntry is not null)
            {
                break;
            }

            timeline.Add(new OrderTimelineEntry
            {
                Status = status.ToString(),
                Label = label,
                State = "pending",
                Timestamp = null,
                Detail = null
            });
        }

        if (cancelledEntry is not null)
        {
            timeline.Add(new OrderTimelineEntry
            {
                Status = OrderStatus.Cancelled.ToString(),
                Label = "Order cancelled",
                State = "failed",
                Timestamp = cancelledEntry.StatusChangedOnUtc,
                Detail = cancelledEntry.Reason
            });
        }

        return timeline;
    }

    private static Dictionary<OrderStatus, OrderStatusHistory> BuildFallbackHistory(eShop.Ordering.Domain.AggregatesModel.OrderAggregate.Order order)
    {
        var fallbackTimestamp = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
        var history = new Dictionary<OrderStatus, OrderStatusHistory>
        {
            [OrderStatus.Submitted] = new(order.Id, OrderStatus.Submitted, "Order submitted", fallbackTimestamp)
        };

        var currentIndex = Array.FindIndex(TimelineSequence, entry => entry.Status == order.OrderStatus);
        if (currentIndex >= 0)
        {
            for (var index = 1; index <= currentIndex; index++)
            {
                var status = TimelineSequence[index].Status;
                history[status] = new OrderStatusHistory(order.Id, status, status == order.OrderStatus ? order.Description : null, fallbackTimestamp);
            }
        }
        else if (order.OrderStatus == OrderStatus.Cancelled)
        {
            history[OrderStatus.Cancelled] = new OrderStatusHistory(order.Id, OrderStatus.Cancelled, order.Description, fallbackTimestamp);
        }

        return history;
    }
}
