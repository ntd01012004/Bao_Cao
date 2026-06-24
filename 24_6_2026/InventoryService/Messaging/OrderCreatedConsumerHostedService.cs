using System.Text;
using System.Text.Json;
using InventoryService.Messaging;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace InventoryService.Messaging;

public sealed class OrderCreatedConsumerHostedService(
    IOptions<RabbitMqOptions> options,
    InventoryStore inventoryStore,
    ILogger<OrderCreatedConsumerHostedService> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Could not connect to RabbitMQ. Retrying in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private Task ListenAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Direct, durable: true);
        channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.QueueName, _options.ExchangeName, _options.RoutingKey);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += HandleMessageAsync;
        channel.BasicConsume(_options.QueueName, autoAck: true, consumer);

        logger.LogInformation(
            "InventoryService is listening on queue {QueueName} via exchange {ExchangeName}.",
            _options.QueueName,
            _options.ExchangeName);

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task HandleMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

            if (orderEvent?.Data is null || !string.Equals(orderEvent.EventName, "OrderCreated", StringComparison.Ordinal))
            {
                logger.LogWarning("Ignored unsupported message: {Payload}", json);
                return Task.CompletedTask;
            }

            var reservation = new InventoryReservation(
                orderEvent.Data.Id,
                orderEvent.Data.ProductId,
                orderEvent.Data.Quantity,
                orderEvent.Data.CustomerName,
                "Reserved",
                DateTime.UtcNow);

            inventoryStore.AddReservation(reservation);
            logger.LogInformation(
                "Reserved inventory for order {OrderId}, product {ProductId}, quantity {Quantity}.",
                reservation.OrderId,
                reservation.ProductId,
                reservation.Quantity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process RabbitMQ message.");
        }

        return Task.CompletedTask;
    }
}
