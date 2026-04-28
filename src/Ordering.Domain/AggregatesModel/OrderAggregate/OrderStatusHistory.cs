namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

#nullable enable

/// <summary>
/// Tracks order status transitions with timestamps and metadata.
/// Used to build the order timeline displayed to customers.
/// </summary>
public class OrderStatusHistory : Entity
{
    /// <summary>
    /// The order this status history entry belongs to.
    /// </summary>
    public int OrderId { get; private set; }

    /// <summary>
    /// The status at this point in the order lifecycle.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// When this status transition occurred.
    /// </summary>
    public DateTime StatusChangedOnUtc { get; private set; }

    /// <summary>
    /// Optional description of what triggered this status change.
    /// </summary>
    public string? Reason { get; private set; }

    protected OrderStatusHistory()
    {
        // EF Core requires a parameterless constructor
    }

    /// <summary>
    /// Creates a new status history entry for an order transition.
    /// </summary>
    /// <param name="orderId">The ID of the order</param>
    /// <param name="status">The new status</param>
    /// <param name="reason">Optional description of the status change</param>
    public OrderStatusHistory(int orderId, OrderStatus status, string? reason = null, DateTime? statusChangedOnUtc = null)
    {
        OrderId = orderId;
        Status = status;
        StatusChangedOnUtc = statusChangedOnUtc ?? DateTime.UtcNow;
        Reason = reason;
    }
}
