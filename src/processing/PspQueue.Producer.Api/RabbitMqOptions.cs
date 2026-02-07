namespace PspQueue.Producer.Api;

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
}
