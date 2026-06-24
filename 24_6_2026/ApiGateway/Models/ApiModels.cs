namespace ApiGateway.Models;

public sealed record ProductDto(int Id, string Name, double Price, int Stock);

public sealed record CreateOrderRequest(int ProductId, int Quantity, string CustomerName);

public sealed record OrderDto(
    Guid Id,
    int ProductId,
    int Quantity,
    string CustomerName,
    DateTime CreatedAtUtc);

public sealed record InventoryReservationDto(
    Guid OrderId,
    int ProductId,
    int Quantity,
    string CustomerName,
    string Status,
    DateTime ProcessedAtUtc);
