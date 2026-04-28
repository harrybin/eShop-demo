namespace eShop.WebApp.Services;

public static class OrderStatusSemantics
{
    public static StatusChipPresentation BuildCurrentStatus(OrderDetailsRecord order)
    {
        var latestCompleted = order.Timeline
            .LastOrDefault(entry => !string.Equals(entry.State, "pending", StringComparison.OrdinalIgnoreCase));

        var label = latestCompleted?.Label ?? order.Status;
        var chipState = order.Status switch
        {
            "Submitted" or "AwaitingValidation" => "processing",
            "StockConfirmed" or "Paid" or "Shipped" => "confirmed",
            "Cancelled" => "failed",
            _ => "pending"
        };

        return new StatusChipPresentation(label, chipState);
    }

    public static string BuildTimelineStateClass(OrderTimelineEntryRecord entry) => entry.State switch
    {
        "completed" => "complete",
        "failed" => "failed",
        _ => "pending"
    };
}

public sealed record StatusChipPresentation(string Label, string State);
