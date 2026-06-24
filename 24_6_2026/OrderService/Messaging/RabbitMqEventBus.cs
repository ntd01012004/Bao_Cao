using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OrderService.Models;
using RabbitMQ.Client;

namespace OrderService.Messaging;

public sealed class RabbitMqEventBus(IOptions<RabbitMqOptions> options) : IEventBus
{
    private readonly RabbitMqOptions _options = options.Value;

    public Task PublishOrderCreatedAsync(Order order, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Direct, durable: true);
        channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.QueueName, _options.ExchangeName, _options.RoutingKey);

        var payload = JsonSerializer.Serialize(new
        {
            EventName = "OrderCreated",
            Data = order
        });

        var body = Encoding.UTF8.GetBytes(payload);
        var properties = channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2;

        channel.BasicPublish(_options.ExchangeName, _options.RoutingKey, properties, body);
        return Task.CompletedTask;
    }
}
