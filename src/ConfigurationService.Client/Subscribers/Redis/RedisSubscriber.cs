using System;
using System.IO;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using RedisOptions = StackExchange.Redis.ConfigurationOptions;

namespace ConfigurationService.Client.Subscribers.Redis
{
    public class RedisSubscriber : ISubscriber
    {
        private readonly ILogger _logger;

        private readonly RedisOptions _options;
        private static ConnectionMultiplexer _connection;

        public string Name => "Redis";

        public RedisSubscriber(string configuration)
        {
            _logger = Logger.CreateLogger<RedisSubscriber>();

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _options = RedisOptions.Parse(configuration);

        }

        public RedisSubscriber(RedisOptions configurationOptions)
        {
            _logger = Logger.CreateLogger<RedisSubscriber>();

            _options = configurationOptions ?? throw new ArgumentNullException(nameof(configurationOptions));
        }

        public void Initialize()
        {
            using (var writer = new StringWriter())
            {
                _connection = ConnectionMultiplexer.Connect(_options, writer);

                _logger.LogDebug(writer.ToString());
            }

            _connection.ErrorMessage += (sender, args) => { _logger.LogError(args.Message); };

            _connection.ConnectionFailed += (sender, args) => { _logger.LogError(args.Exception, "Redis connection failed."); };

            _connection.ConnectionRestored += (sender, args) => { _logger.LogInformation("Redis connection restored."); };
        }

        public void Subscribe(string channel, Action<string> handler)
        {
            _logger.LogInformation("Subscribing to Redis channel '{channel}'.", channel);

            var subscriber = _connection.GetSubscriber();

            subscriber.Subscribe(channel, (redisChannel, value) =>
            {
                _logger.LogInformation("Received subscription on Redis channel '{channel}'.", channel);

                handler(value);
            });

            var endpoint = subscriber.SubscribedEndpoint(channel);
            _logger.LogInformation("Subscribed to Redis endpoint {endpoint} for channel '{channel}'.", endpoint, channel);
        }
    }
}