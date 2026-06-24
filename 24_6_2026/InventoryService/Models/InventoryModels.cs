namespace InventoryService.Models;

public sealed record InventoryReservation(
    Guid OrderId,
    int ProductId,
    int Quantity,
    string CustomerName,
    string Status,
    DateTime ProcessedAtUtc);

public sealed record OrderCreatedPayload(
    Guid Id,
    int ProductId,
    int Quantity,
    string CustomerName,
    DateTime CreatedAtUtc);

public sealed record OrderCreatedEvent(string EventName, OrderCreatedPayload Data);
