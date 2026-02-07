using System.Text;
using System.Text.Json;

using Imonas.Exchange.Contracts;

using Microsoft.Extensions.Options;

using PspConnectors;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PspQueue.Consumer.Worker;

public sealed class QueueConsumerWorker : BackgroundService
{
    private const string RetryHeaderName = "x-retry-count";
    private readonly IRabbitMqConnectionProvider _connectionProvider;
    private readonly TransactionsHandler _transactionsHandler;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<QueueConsumerWorker> _logger;

    private IModel? _channel;

    public QueueConsumerWorker(
        IRabbitMqConnectionProvider connectionProvider,
        TransactionsHandler transactionsHandler,
        IOptions<RabbitMqOptions> options,
        ILogger<QueueConsumerWorker> logger
        )
    {
        _connectionProvider = connectionProvider;
        _transactionsHandler = transactionsHandler;
        _options = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogInformation("Stopping consumer worker"));

        SetupChannel();

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.Received += OnMessageReceivedAsync;

        _channel!.BasicQos(0, _options.PrefetchCount, false);
        _channel.BasicConsume(queue: _options.Queue, autoAck: false, consumer: consumer);

        _logger.LogInformation("Started consuming from queue {Queue}", _options.Queue);

        return Task.CompletedTask;
    }

    private void SetupChannel()
    {
        var connection = _connectionProvider.GetConnection();
        _channel?.Dispose();
        _channel = connection.CreateModel();

        // Exchanges
        _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.ExchangeDeclare(_options.RetryExchange, ExchangeType.Direct, durable: true, autoDelete: false);
        _channel.ExchangeDeclare(_options.DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false);

        _channel.QueueDeclare(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(_options.Queue, _options.Exchange, _options.RoutingKey);

        // Retry queue with TTL and DLX back to main
        var retryArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = _options.Exchange,
            ["x-dead-letter-routing-key"] = _options.RoutingKey,
            ["x-message-ttl"] = _options.RetryDelayMs
        };

        _channel.QueueDeclare(
            queue: _options.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: retryArgs);

        _channel.QueueBind(_options.RetryQueue, _options.RetryExchange, _options.RetryQueue);

        // Dead-letter queue (simple)
        _channel.QueueDeclare(
            queue: _options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(_options.DeadLetterQueue, _options.DeadLetterExchange, _options.DeadLetterQueue);

        _logger.LogInformation("RabbitMQ topology initialized");
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        if (_channel is null)
            return;

        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        QueuePspData? message;
        try
        {
            message = JsonSerializer.Deserialize<QueuePspData>(json);
            if (message is null)
                throw new InvalidOperationException("Deserialized message is null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message. Sending to DLQ");
            SendToDeadLetter(ea, json, null);
            _channel.BasicAck(ea.DeliveryTag, multiple: false);
            return;
        }

        var correlationId = message.correlationId.ToString();
        var retryCount = GetRetryCount(ea.BasicProperties);

        try
        {
            _logger.LogInformation(
                "Processing message for PSP {PspName} ({CorrelationId}) Attempt {Attempt}",
                message.pspId,
                correlationId,
                retryCount + 1);

            await ProcessMessageAsync(message);

            if (message.correlationId == Guid.Empty)
            {
                throw new InvalidOperationException("Missing CorrelationId" + System.Text.Json.JsonSerializer.Serialize(message));
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing message {CorrelationId} on attempt {Attempt}",
                correlationId, retryCount + 1);

            if (retryCount + 1 >= _options.RetryMaxAttempts)
            {
                _logger.LogWarning("Max retries reached for {CorrelationId}. Sending to DLQ", correlationId);
                SendToDeadLetter(ea, json, retryCount);
            }
            else
            {
                _logger.LogInformation("Scheduling retry {NextAttempt} for {CorrelationId}",
                    retryCount + 2, correlationId);
                SendToRetry(ea, json, retryCount);
            }

            _channel.BasicAck(ea.DeliveryTag, multiple: false);
        }
    }

    private async Task ProcessMessageAsync(QueuePspData data)
    {
        await _transactionsHandler.ProcessRun(data.startDate, data.endDate, data.externalSource, data.pspId);
    }

    private static int GetRetryCount(IBasicProperties props)
    {
        if (props.Headers is null) return 0;

        if (props.Headers.TryGetValue(RetryHeaderName, out var value) &&
            value is byte[] raw &&
            int.TryParse(Encoding.UTF8.GetString(raw), out var retry))
        {
            return retry;
        }

        return 0;
    }

    private void SendToRetry(BasicDeliverEventArgs ea, string json, int currentRetry)
    {
        var props = _channel!.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.CorrelationId = ea.BasicProperties.CorrelationId ?? Guid.NewGuid().ToString();

        props.Headers ??= new Dictionary<string, object>();
        props.Headers[RetryHeaderName] = Encoding.UTF8.GetBytes((currentRetry + 1).ToString());

        _channel.BasicPublish(
            exchange: _options.RetryExchange,
            routingKey: _options.RetryQueue,
            basicProperties: props,
            body: Encoding.UTF8.GetBytes(json));
    }

    private void SendToDeadLetter(BasicDeliverEventArgs ea, string json, int? retryCount)
    {
        var props = _channel!.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.CorrelationId = ea.BasicProperties.CorrelationId ?? Guid.NewGuid().ToString();

        props.Headers ??= new Dictionary<string, object>();
        props.Headers[RetryHeaderName] = Encoding.UTF8.GetBytes((retryCount ?? 0).ToString());

        _channel.BasicPublish(
            exchange: _options.DeadLetterExchange,
            routingKey: _options.DeadLetterQueue,
            basicProperties: props,
            body: Encoding.UTF8.GetBytes(json));
    }

    public override void Dispose()
    {
        base.Dispose();
        _channel?.Dispose();
    }
}
