using System;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConfigurationService.Client.Subscribers.RabbitMq
{
    public class RabbitMqSubscriber : ISubscriber
    {
        private readonly ILogger _logger;

        private readonly RabbitMqOptions _options;
        private string _exchangeName;
        private static IModel _channel;

        public string Name => "RabbitMQ";

        public RabbitMqSubscriber(RabbitMqOptions options)
        {
            _logger = Logger.CreateLogger<RabbitMqSubscriber>();

            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Initialize()
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _exchangeName = _options.ExchangeName;

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            connection.CallbackException += (sender, args) => { _logger.LogError(args.Exception, "RabbitMQ callback exception"); };

            connection.ConnectionBlocked += (sender, args) => { _logger.LogError("RabbitMQ connection is blocked. Reason: {Reason}", args.Reason); };

            connection.ConnectionShutdown += (sender, args) => { _logger.LogError("RabbitMQ connection was shut down. Reason: {ReplyText}", args.ReplyText); };

            connection.ConnectionUnblocked += (sender, args) => { _logger.LogInformation("RabbitMQ connection was unblocked"); };

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout);
        }

        public void Subscribe(string routingKey, Action<string> handler)
        {
            _logger.LogInformation("Binding to RabbitMQ queue with routing key '{RoutingKey}'", routingKey);

            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queueName, _exchangeName, routingKey);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, args) =>
            {
                _logger.LogInformation("Received message with routing key '{RoutingKey}'", args.RoutingKey);

                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                handler(message);
            };

            var consumerTag = _channel.BasicConsume(queueName, true, consumer);

            _logger.LogInformation("Consuming RabbitMQ queue {QueueName} for consumer '{ConsumerTag}'", queueName, consumerTag);
        }
    }
}