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

        private readonly Options _options;
        private static IConnection _connection;

        public NatsPublisher(ILogger<NatsPublisher> logger, Options options)
        {
            _logger = logger;

            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Initialize()
        {
            _options.AsyncErrorEventHandler += (sender, args) => { _logger.LogError(args.Error); };

            _options.ClosedEventHandler += (sender, args) => { _logger.LogError(args.Error, "NATS connection was closed."); };

            _options.DisconnectedEventHandler += (sender, args) => { _logger.LogError(args.Error, "NATS connection was disconnected."); };

            _options.ReconnectedEventHandler += (sender, args) => { _logger.LogInformation("NATS connection was restored."); };

            var connectionFactory = new ConnectionFactory();
            _connection = connectionFactory.CreateConnection(_options);

            _logger.LogInformation("NATS publisher initialized.");
        }

        public Task Publish(string subject, string message)
        {
            _logger.LogInformation("Publishing message to with subject {subject}.", subject);

            var data = Encoding.UTF8.GetBytes(message);

            _connection.Publish(subject, data);

            return Task.CompletedTask;
        }
    }
}