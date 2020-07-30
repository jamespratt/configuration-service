using System;
using System.IO;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ConfigurationService.Client.Subscribers.Redis
{
    public class RedisSubscriber : ISubscriber
    {
        private readonly ILogger _logger;

        private static ConnectionMultiplexer _connection;

        public string Name => "Redis";

        public RedisSubscriber(string configuration)
        {
            _logger = Logger.CreateLogger<RedisSubscriber>();

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var configurationOptions = ConfigurationOptions.Parse(configuration);

            CreateConnection(configurationOptions);

        }

        public RedisSubscriber(ConfigurationOptions configurationOptions)
        {
            _logger = Logger.CreateLogger<RedisSubscriber>();

            if (configurationOptions == null)
            {
                throw new ArgumentNullException(nameof(configurationOptions));
            }

            CreateConnection(configurationOptions);
        }

        public void Subscribe(string topic, Action<string> handler)
        {
            _logger.LogInformation("Subscribing to Redis channel '{topic}'.", topic);

            var subscriber = _connection.GetSubscriber();

            subscriber.Subscribe(topic, (channel, message) =>
            {
                _logger.LogInformation("Received subscription on Redis channel '{channel}'.", channel);

                handler(message);
            });

            var endpoint = subscriber.SubscribedEndpoint(topic);
            _logger.LogInformation("Subscribed to Redis endpoint {endpoint} for channel '{topic}'.", endpoint, topic);
        }

        private void CreateConnection(ConfigurationOptions configurationOptions)
        {
            using (var writer = new StringWriter())
            {
                _connection = ConnectionMultiplexer.Connect(configurationOptions, writer);

                _logger.LogDebug(writer.ToString());
            }

            _connection.ErrorMessage += (sender, args) => { _logger.LogError(args.Message); };

            _connection.ConnectionFailed += (sender, args) => { _logger.LogError(args.Exception, "Redis connection failed."); };

            _connection.ConnectionRestored += (sender, args) => { _logger.LogInformation("Redis connection restored."); };
        }
    }
}