namespace PspQueue.Consumer.Worker;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "psps.exchange";
    public string RoutingKey { get; set; } = "psps.route";
    public string Queue { get; set; } = "psps.queue";
    public string RetryExchange { get; set; } = "psps.retry.exchange";
    public string RetryQueue { get; set; } = "psps.queue.retry";
    public string DeadLetterExchange { get; set; } = "psps.dlx";
    public string DeadLetterQueue { get; set; } = "psps.queue.dlq";
    public int RetryMaxAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 10000;
    public ushort PrefetchCount { get; set; } = 10;
}
