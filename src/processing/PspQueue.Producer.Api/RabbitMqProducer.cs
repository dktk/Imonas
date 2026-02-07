using System.Text;
using System.Text.Json;

using Imonas.Exchange.Contracts;

using Microsoft.Extensions.Options;

using RabbitMQ.Client;

namespace PspQueue.Producer.Api;

public interface IQueuePspDataProducer
{
    void Publish(QueuePspData message);
}

public sealed class QueuePspDataProducer : IQueuePspDataProducer, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<QueuePspDataProducer> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public QueuePspDataProducer(
        IOptions<RabbitMqOptions> options,
        ILogger<QueuePspDataProducer> logger)
    {
        _options = options.Value;
        _logger = logger;

        EnsureChannel();
        DeclareTopology();
    }

    private void EnsureChannel()
    {
        if (_channel is { IsOpen: true }) return;

        lock (_lock)
        {
            if (_channel is { IsOpen: true }) return;

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _connection?.Dispose();
            _channel?.Dispose();

            _connection = factory.CreateConnection("psps-producer");
            _channel = _connection.CreateModel();
        }
    }

    private void DeclareTopology()
    {
        _channel!.ExchangeDeclare(
            exchange: _options.Exchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        _channel.QueueDeclare(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: _options.Queue,
            exchange: _options.Exchange,
            routingKey: _options.RoutingKey);
    }

    public void Publish(QueuePspData message)
    {
        EnsureChannel();

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel!.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.CorrelationId = message.correlationId.ToString();

        _channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: _options.RoutingKey,
            basicProperties: props,
            body: body);

        _logger.LogInformation(
            "Published message for PSP {PspName} with CorrelationId {CorrelationId}",
            message.pspId,
            message.correlationId);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
