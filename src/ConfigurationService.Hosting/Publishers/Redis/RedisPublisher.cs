using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ConfigurationService.Hosting.Publishers.Redis;

public class RedisPublisher : IPublisher
{
    private readonly ILogger<RedisPublisher> _logger;

    private readonly ConfigurationOptions _options;
    private static IConnectionMultiplexer _connection;

    public RedisPublisher(ILogger<RedisPublisher> logger, ConfigurationOptions configuration)
    {
        _logger = logger;

        _options = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void Initialize()
    {

        using (var writer = new StringWriter())
        {
            _connection = ConnectionMultiplexer.Connect(_options, writer);

            _logger.LogTrace("Redis publisher connected with log:\r\n{Log}", writer);
        }

        _connection.ErrorMessage += (sender, args) => { _logger.LogError("Redis replied with an error message: {Message}", args.Message); };

        _connection.ConnectionFailed += (sender, args) => { _logger.LogError(args.Exception, "Redis connection failed"); };

        _connection.ConnectionRestored += (sender, args) => { _logger.LogInformation("Redis connection restored"); };

        _logger.LogInformation("Redis publisher initialized");
    }

    public async Task Publish(string topic, string message)
    {
        _logger.LogInformation("Publishing message to channel {Channel}", topic);

        var publisher = _connection.GetSubscriber();

        var clientCount = await publisher.PublishAsync(topic, message);

        _logger.LogInformation("Message to channel {Channel} was received by {ClientCount} clients", topic, clientCount);
    }
}