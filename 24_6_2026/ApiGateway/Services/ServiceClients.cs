using ApiGateway.Models;

namespace ApiGateway.Services;

public interface IOrderServiceClient
{
    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}

public interface IInventoryServiceClient
{
    Task<IReadOnlyList<InventoryReservationDto>> GetReservationsAsync(CancellationToken cancellationToken);
}
