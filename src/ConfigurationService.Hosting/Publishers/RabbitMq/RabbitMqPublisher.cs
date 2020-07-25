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

        private static IModel _channel;

        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, RabbitMqOptions options)
        {
            _logger = logger;

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                VirtualHost = options.VirtualHost,
                UserName =  options.UserName,
                Password = options.Password
            };

            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();

            connection.CallbackException += (sender, args) => { _logger.LogError(args.Exception, "RabbitMQ callback exception."); };

            connection.ConnectionBlocked += (sender, args) => { _logger.LogError("RabbitMQ connection is blocked. Reason: {Reason}", args.Reason); };

            connection.ConnectionShutdown += (sender, args) => { _logger.LogError("RabbitMQ connection was shut down. Reason: {ReplyText}", args.ReplyText); };

            connection.ConnectionUnblocked += (sender, args) => { _logger.LogInformation("RabbitMQ connection was unblocked."); };

            _channel.ExchangeDeclare("configuration-service", ExchangeType.Fanout);

        }

        public Task Publish(string topic, string message)
        {
            _logger.LogInformation("Publishing message with routing key {topic}.", topic);

            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish("configuration-service", topic, null, body);

            return Task.CompletedTask;
        }
    }
}