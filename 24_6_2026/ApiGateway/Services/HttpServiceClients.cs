using System.Net.Http.Json;
using ApiGateway.Models;

namespace ApiGateway.Services;

public sealed class OrderServiceClient(HttpClient httpClient) : IOrderServiceClient
{
    public async Task<IReadOnlyList<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await httpClient.GetFromJsonAsync<List<OrderDto>>("api/orders", cancellationToken);
        return orders ?? [];
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/orders", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderDto>(cancellationToken))!;
    }
}

public sealed class InventoryServiceClient(HttpClient httpClient) : IInventoryServiceClient
{
    public async Task<IReadOnlyList<InventoryReservationDto>> GetReservationsAsync(CancellationToken cancellationToken)
    {
        var reservations = await httpClient.GetFromJsonAsync<List<InventoryReservationDto>>(
            "api/inventory/reservations",
            cancellationToken);
        return reservations ?? [];
    }
}
