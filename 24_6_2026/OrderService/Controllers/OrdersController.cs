using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using OrderService.Messaging;
using OrderService.Models;

namespace OrderService.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IEventBus eventBus) : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, Order> Orders = [];

    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetOrders()
    {
        return Ok(Orders.Values.OrderByDescending(order => order.CreatedAtUtc));
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0 || request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.CustomerName))
        {
            return BadRequest("ProductId, Quantity and CustomerName are required.");
        }

        var order = new Order(
            Guid.NewGuid(),
            request.ProductId,
            request.Quantity,
            request.CustomerName.Trim(),
            DateTime.UtcNow);

        await eventBus.PublishOrderCreatedAsync(order, cancellationToken);
        Orders[order.Id] = order;

        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }
}
