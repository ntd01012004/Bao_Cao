using OrderService.Models;

namespace OrderService.Messaging;

public interface IEventBus
{
    Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken);
}
