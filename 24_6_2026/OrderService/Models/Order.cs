namespace OrderService.Models;

public sealed record Order(
    Guid Id,
    int ProductId,
    int Quantity,
    string CustomerName,
    DateTime CreatedAtUtc);
