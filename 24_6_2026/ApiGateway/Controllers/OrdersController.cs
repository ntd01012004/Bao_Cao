using ApiGateway.Models;
using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderServiceClient orderServiceClient) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await orderServiceClient.GetOrdersAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orderServiceClient.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}
