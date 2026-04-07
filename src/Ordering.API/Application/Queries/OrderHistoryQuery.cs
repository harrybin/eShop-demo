namespace eShop.Ordering.API.Application.Queries;

public class OrderHistoryQuery
{
    public OrderStatus? Status { get; init; }

    public DateTime? FromDateUtc { get; init; }

    public DateTime? ToDateUtc { get; init; }

    public int? PageNumber { get; init; } = 1;

    public int? PageSize { get; init; } = 20;
}
