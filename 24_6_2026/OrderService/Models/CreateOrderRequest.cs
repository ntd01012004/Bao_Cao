namespace OrderService.Models;

public sealed record CreateOrderRequest(int ProductId, int Quantity, string CustomerName);
