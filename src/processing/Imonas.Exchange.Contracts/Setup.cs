// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Imonas.Exchange.Contracts
{
    public class RabbitMqTopology
    {
        public string Exchange { get; set; }
        public string Queue { get; set; }
        public string RoutingKey { get; set; }

        public string RetryExchange { get; set; }
        public string RetryQueue { get; set; }
        public string RetryRoutingKey { get; set; }

        public string DlxExchange { get; set; }
        public string DlqQueue { get; set; }
        public string DlqRoutingKey { get; set; }

        public static RabbitMqTopology Create(string exchange, string target)
        {
            return new RabbitMqTopology
            {
                Exchange = exchange,
                Queue = $"{target}.queue",
                RoutingKey = $"{target}.route",

                RetryExchange = $"{target}.retry.exchange",
                RetryQueue = $"{target}.queue.retry",
                RetryRoutingKey = $"{target}.route.retry",

                DlxExchange = $"{target}.dlx",
                DlqQueue = $"{target}.queue.dlq",
                DlqRoutingKey = $"{target}.route.dlq"
            };
        }
    }
}
