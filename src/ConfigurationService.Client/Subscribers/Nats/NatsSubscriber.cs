using System;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace ConfigurationService.Client.Subscribers.Nats
{
    public class NatsSubscriber : ISubscriber
    {
        private readonly ILogger _logger;

        private readonly Options _options;

        private static IConnection _connection;

        public string Name => "NATS";

        public NatsSubscriber(Options options)
        {
            _logger = Logger.CreateLogger<NatsSubscriber>();

            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public void Initialize()
        {
            _options.AsyncErrorEventHandler += (sender, args) => { _logger.LogError("NATS replied with an error message: {Message}", args.Error); };

            _options.ClosedEventHandler += (sender, args) => { _logger.LogError(args.Error, "NATS connection was closed"); };

            _options.DisconnectedEventHandler += (sender, args) => { _logger.LogError(args.Error, "NATS connection was disconnected"); };

            _options.ReconnectedEventHandler += (sender, args) => { _logger.LogInformation("NATS connection was restored"); };

            var connectionFactory = new ConnectionFactory();
            _connection = connectionFactory.CreateConnection(_options);
        }

        public void Subscribe(string subject, Action<string> handler)
        {
            _logger.LogInformation("Subscribing to NATS subject '{Subject}'", subject);

            _connection.SubscribeAsync(subject, (sender, args) =>
            {
                _logger.LogInformation("Received subscription on NATS subject '{Subject}'", subject);

                var message = args.Message.ToString();

                handler(message);
            });

            _logger.LogInformation("Subscribed to NATS subject '{Subject}'", subject);
        }
    }
}