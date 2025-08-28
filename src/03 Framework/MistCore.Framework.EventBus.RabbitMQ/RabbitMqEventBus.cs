using Microsoft.Extensions.Logging;
using MistCore.Framework.EventBus.RabbitMQ.Config;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MistCore.Framework.EventBus.RabbitMQ
{
    public class RabbitMqEventBus : IEventBus
    {
        private ILogger<RabbitMqEventBus> logger;

        private readonly IEventHandlerExecutionContext eventHandlerExecutionContext;
        private readonly IConnectionFactory connectionFactory;
        private IConnection _connection;
        private IModel channel;

        private string exchangeName;
        private string exchangeType;
        private string deadLetterExchange;
        private string deadLetterRoutingKey;

        private string queueName;
        private int? queueMaxLength = 50000;
        private string queueOverflow = "drop-head";
        private int? queueMessageTTL = 18000;
        private bool consumeAutoAck;
        private int? messagEexpiration;
        private int? messagMmaxAttempts;

        public RabbitMqEventBus(EventBusInfo eventBusInfo, IEventHandlerExecutionContext eventHandlerExecutionContext, ILoggerFactory loggerFactory)
        {
            this.eventHandlerExecutionContext = eventHandlerExecutionContext;
            this.connectionFactory = new ConnectionFactory { HostName = eventBusInfo.Host, Port = eventBusInfo.Port };
            if (!string.IsNullOrEmpty(eventBusInfo.Username))
            {
                this.connectionFactory.UserName = eventBusInfo.Username;
            }
            if (!string.IsNullOrEmpty(eventBusInfo.Password))
            {
                this.connectionFactory.Password = eventBusInfo.Password;
            }

            this.exchangeName = eventBusInfo.ExchangeName;
            this.exchangeType = eventBusInfo.ExchangeType;
            this.deadLetterExchange = eventBusInfo.DeadLetterExchange;
            this.deadLetterRoutingKey = eventBusInfo.DeadLetterRoutingKey;
            this.queueName = eventBusInfo.QueueName;
            //this.queueMaxLength = eventBusInfo.QueueMaxLength;
            //this.queueOverflow = eventBusInfo.QueueOverflow;
            //this.queueMessageTTL = eventBusInfo.QueueMessageTTL;
            //this.consumeAutoAck = eventBusInfo.ConsumeAutoAck;
            //this.messagEexpiration = eventBusInfo.MessagEexpiration;
            this.messagMmaxAttempts = eventBusInfo.MessageMaxAttempts;
            this.logger = loggerFactory.CreateLogger<RabbitMqEventBus>();

            Receive();
        }

        public IConnection GetConnection()
        {
            if (this._connection != null && this._connection.IsOpen)
            {
                return this._connection;
            }

            var retryPolicy = RetryPolicy.Handle<SocketException>().Or<BrokerUnreachableException>()
                .WaitAndRetry(5, p => TimeSpan.FromSeconds(1), (ex, time) => {
                    this.logger.LogError(ex, $"RabbitMQ EventBus connection error {time.TotalMilliseconds} ms.");
                });

            retryPolicy.Execute(() =>
            {
                this._connection = this.connectionFactory.CreateConnection();
            });

            return _connection;
        }

        public void Receive()
        {
            var connection = GetConnection();
            var channel = connection.CreateModel();
            this.channel = channel;

            //channel.ExchangeDeclare(exchange: deadLetterExchange, type: "direct", durable: true);
            //channel.QueueDeclare(queue: "tttt", durable: true, exclusive: false, autoDelete: false, arguments: null);
            //channel.QueueBind("tttt", deadLetterExchange, deadLetterRoutingKey);

            channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true);

            var deadLetterParams = new Dictionary<string, object>();
            deadLetterParams.Add("x-max-length", queueMaxLength);
            deadLetterParams.Add("x-message-ttl", queueMessageTTL);
            deadLetterParams.Add("x-overflow", queueOverflow);
            if (!string.IsNullOrEmpty(deadLetterExchange))
            {
                deadLetterParams.Add("x-dead-letter-exchange", deadLetterExchange);
                deadLetterParams.Add("x-dead-letter-routing-key", deadLetterRoutingKey);
            }
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: deadLetterParams);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var result = await this.eventHandlerExecutionContext.HandleAsync(ea.RoutingKey, json);

                if (!consumeAutoAck)
                {
                    if (result)
                    {
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                    }
                    else
                    {
                        Resend(channel, ea);
                    }
                }
            };
            channel.BasicConsume(queue: this.queueName, autoAck: consumeAutoAck, consumer: consumer);
            var list = this.eventHandlerExecutionContext.GetAll();
            foreach (var item in list)
            {
                channel.QueueBind(this.queueName, this.exchangeName, item.Item1);
            }

            channel.CallbackException += (sender, ea) =>
            {
                if (channel != null && channel.IsOpen)
                {
                    channel.Close();
                    channel.Dispose();
                }
                Receive();
            };
        }

        private void Resend(IModel channel, BasicDeliverEventArgs ea)
        {
            var properties = ea.BasicProperties;
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();

            var retryCount = (properties.Headers != null && properties.Headers.ContainsKey("retryCount")) ? (int)ea.BasicProperties.Headers["retryCount"] : 0;

            if (this.messagMmaxAttempts == null || retryCount > this.messagMmaxAttempts)
            {
                return;
            }

            properties.Headers["retryCount"] = ++retryCount;

            try
            {
                channel.BasicReject(ea.DeliveryTag, false);
                channel.BasicPublish(ea.Exchange, ea.RoutingKey, properties, ea.Body);
            }
            catch (AlreadyClosedException ex)
            {
                logger.LogError("retry error.", ex);
            }
        }

        public void Trigger<TEvent>(string routeKey, TEvent @event)
        {
            using (var channel = GetConnection().CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true);

                var message = JsonConvert.SerializeObject(@event, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                var body = Encoding.UTF8.GetBytes(message);

                IBasicProperties basicProperties = null;
                if (messagEexpiration != null)
                {
                    basicProperties = channel.CreateBasicProperties();
                    if (messagEexpiration == -1)
                    {
                        basicProperties.DeliveryMode = 2;
                    }
                    else
                    {
                        basicProperties.Expiration = messagEexpiration.ToString();
                    }
                }
                channel.BasicPublish(this.exchangeName, routeKey, basicProperties, body);
            }
        }

        public void Register<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>
        {
            if (this.eventHandlerExecutionContext.RegisterEventHandler<TEvent, TEventHandler>(routeKey))
            {
                this.channel.QueueBind(this.queueName, this.exchangeName, routeKey);
            }
        }

        public void UnRegister<TEvent, TEventHandler>(string routeKey) where TEventHandler : IEventHandler<TEvent>
        {
            if (this.eventHandlerExecutionContext.UnRegisterEventHandler<TEvent, TEventHandler>(routeKey))
            {
                this.channel.QueueUnbind(this.queueName, this.exchangeName, routeKey);
            }
        }

    }
}
