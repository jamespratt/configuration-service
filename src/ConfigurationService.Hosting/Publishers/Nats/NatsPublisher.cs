using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace ConfigurationService.Hosting.Publishers.Nats
{
    public class NatsPublisher : IPublisher
    {
        private readonly ILogger<NatsPublisher> _logger;

        private static IConnection _connection;

        public NatsPublisher(ILogger<NatsPublisher> logger, Options options)
        {
            _logger = logger;

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.AsyncErrorEventHandler += (sender, args) => { _logger.LogError(args.Error); };

            options.ClosedEventHandler += (sender, args) => { _logger.LogError(args.Error, "NATS connection was closed."); };

            options.DisconnectedEventHandler += (sender, args) => { _logger.LogError(args.Error, "NATS connection was disconnected."); };

            options.ReconnectedEventHandler += (sender, args) => { _logger.LogInformation("NATS connection was restored."); };

            var connectionFactory = new ConnectionFactory();
            _connection = connectionFactory.CreateConnection(options);
        }

        public Task Publish(string topic, string message)
        {
            _logger.LogInformation("Publishing message to with subject {topic}.", topic);

            var data = Encoding.UTF8.GetBytes(message);

            _connection.Publish(topic, data);

            return Task.CompletedTask;
        }
    }
}