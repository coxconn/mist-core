using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MistCore.Framework.EventBus.RabbitMQ.Config
{
    public class EventBusInfo
    {
        public string Host { get; set; }
        public int Port { get; set; } = 5672;
        public string Username { get; set; }
        public string Password { get; set; }

        public string ExchangeName { get; set; } = "exchange2";
        public string ExchangeType { get; set; } = "direct";
        public string DeadLetterExchange { get; set; }
        public string DeadLetterRoutingKey { get; set; } = "dead";

        public string QueueName { get; set; }
        //public int? QueueMaxLength { get; set; } = 1000;
        //public string QueueOverflow { get; set; } = "drop-head";
        //public int? QueueMessageTTL { get; set; } = 18000;

        //public bool ConsumeAutoAck { get; set; }
        //public int? MessagEexpiration { get; set; }
        public int? MessageMaxAttempts { get; set; } = 3;

    }
}
