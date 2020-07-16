using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ConfigurationService.Publishers.Redis
{
    public class RedisPublisher : IPublisher
    {
        private readonly ILogger<RedisPublisher> _logger;

        private static IConnectionMultiplexer _connection;

        public RedisPublisher(ILogger<RedisPublisher> logger, ConfigurationOptions configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _logger = logger;

            _connection = ConnectionMultiplexer.Connect(configuration);
        }

        public async Task Publish(string topic, string message)
        {
            _logger.LogInformation("Publishing message to channel {topic}.", topic);

            var publisher = _connection.GetSubscriber();

            var clientCount = await publisher.PublishAsync(topic, message);

            _logger.LogInformation("Message to channel {topic} was received by {clientCount} clients.", topic, clientCount);
        }
    }
}