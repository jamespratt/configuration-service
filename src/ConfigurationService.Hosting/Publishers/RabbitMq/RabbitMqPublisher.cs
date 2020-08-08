using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ConfigurationService.Hosting.Publishers.RabbitMq
{
    public class RabbitMqPublisher : IPublisher
    {
        private readonly ILogger<RabbitMqPublisher> _logger;

        private readonly RabbitMqOptions _options;
        private string _exchangeName;
        private static IModel _channel;

        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, RabbitMqOptions options)
        {
            _logger = logger;

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

            connection.CallbackException += (sender, args) => { _logger.LogError(args.Exception, "RabbitMQ callback exception."); };

            connection.ConnectionBlocked += (sender, args) => { _logger.LogError("RabbitMQ connection is blocked. Reason: {Reason}", args.Reason); };

            connection.ConnectionShutdown += (sender, args) => { _logger.LogError("RabbitMQ connection was shut down. Reason: {ReplyText}", args.ReplyText); };

            connection.ConnectionUnblocked += (sender, args) => { _logger.LogInformation("RabbitMQ connection was unblocked."); };

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout);

            _logger.LogInformation("RabbitMQ publisher initialized.");
        }

        public Task Publish(string routingKey, string message)
        {
            _logger.LogInformation("Publishing message with routing key {routingKey}.", routingKey);

            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(_exchangeName, routingKey, null, body);

            return Task.CompletedTask;
        }
    }
}