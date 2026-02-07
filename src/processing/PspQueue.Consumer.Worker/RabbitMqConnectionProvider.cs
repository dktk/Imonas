using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PspQueue.Consumer.Worker;

public interface IRabbitMqConnectionProvider : IDisposable
{
    IConnection GetConnection();
}

public sealed class RabbitMqConnectionProvider : IRabbitMqConnectionProvider
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private IConnection? _connection;
    private readonly object _lock = new();

    public RabbitMqConnectionProvider(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConnectionProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IConnection GetConnection()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        lock (_lock)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _logger.LogInformation("Creating RabbitMQ connection to {Host}:{Port}", _options.HostName, _options.Port);

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection?.Dispose();
            _connection = factory.CreateConnection("psps-consumer");
            return _connection;
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
