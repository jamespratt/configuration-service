using System;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace ConfigurationService.Client.Subscribers.Nats
{
    public class NatsSubscriber : ISubscriber
    {
        private readonly ILogger _logger;

        private static IConnection _connection;

        public string Name => "NATS";

        public NatsSubscriber(Options options)
        {
            _logger = Logger.CreateLogger<NatsSubscriber>();

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

        public void Subscribe(string topic, Action<string> handler)
        {
            var subject = topic;

            _logger.LogInformation("Subscribing to NATS subject '{subject}'.", subject);

            _connection.SubscribeAsync(subject, (sender, args) => 
            {
                _logger.LogInformation("Received subscription on NATS subject '{subject}'.", subject);

                var message = args.Message.ToString();

                handler(message);
            });

            _logger.LogInformation("Subscribed to NATS subject '{subject}'.", subject);
        }
    }
}