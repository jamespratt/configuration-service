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

                _logger.LogTrace("Redis subscriber connected with log:\r\n{Log}", writer);
            }

            _connection.ErrorMessage += (sender, args) => { _logger.LogError("Redis replied with an error message: {Message}", args.Message); };

            _connection.ConnectionFailed += (sender, args) => { _logger.LogError(args.Exception, "Redis connection failed"); };

            _connection.ConnectionRestored += (sender, args) => { _logger.LogInformation("Redis connection restored"); };
        }

        public void Subscribe(string topic, Action<string> handler)
        {
            _logger.LogInformation("Subscribing to Redis channel '{Channel}'", topic);

            var subscriber = _connection.GetSubscriber();

            subscriber.Subscribe(topic, (redisChannel, value) =>
            {
                _logger.LogInformation("Received subscription on Redis channel '{Channel}'", topic);

                handler(value);
            });

            var endpoint = subscriber.SubscribedEndpoint(topic);
            _logger.LogInformation("Subscribed to Redis endpoint {Endpoint} for channel '{Channel}'", endpoint, topic);
        }
    }
}