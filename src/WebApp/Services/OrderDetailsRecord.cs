namespace eShop.WebApp.Services;

public record OrderDetailsRecord(
    int OrderNumber,
    DateTime Date,
    string Status,
    string Description,
    string Street,
    string City,
    string State,
    string Zipcode,
    string Country,
    List<OrderItemRecord> OrderItems,
    decimal Total,
    List<OrderTimelineEntryRecord> Timeline);

public record OrderItemRecord(
    string ProductName,
    int Units,
    double UnitPrice,
    string PictureUrl);

public record OrderTimelineEntryRecord(
    string Status,
    string Label,
    string State,
    DateTime? Timestamp,
    string? Detail);
