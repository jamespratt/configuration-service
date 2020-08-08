using System;
using System.Collections.Generic;
using System.Net.Http;
using ConfigurationService.Client.Subscribers.Nats;
using ConfigurationService.Client.Subscribers.RabbitMq;
using ConfigurationService.Client.Subscribers.Redis;
using Microsoft.Extensions.Logging;
using NATS.Client;
using RedisOptions = StackExchange.Redis.ConfigurationOptions;
using NatsOptions = NATS.Client.Options;

namespace ConfigurationService.Client
{
    public class RemoteConfigurationOptions
    {
        internal IList<ConfigurationOptions> Configurations { get; } = new List<ConfigurationOptions>();

        internal Func<ISubscriber> CreateSubscriber { get; set; }

        /// <summary>
        /// Configuration service endpoint.
        /// </summary>
        public string ServiceUri { get; set; }

        /// <summary>
        /// The <see cref="System.Net.Http.HttpMessageHandler"/> for the <see cref="HttpClient"/>.
        /// </summary>
        public HttpMessageHandler HttpMessageHandler { get; set; }

        /// <summary>
        /// The timeout for the <see cref="HttpClient"/> request to the configuration server.
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// The type used to configure the logging system and create instances of <see cref="ILogger"/>
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// Adds an individual configuration file.
        /// </summary>
        /// <param name="configure">Configures the options for the configuration file.</param>
        public void AddConfiguration(Action<ConfigurationOptions> configure)
        {
            var configurationOptions = new ConfigurationOptions();
            configure(configurationOptions);

            Configurations.Add(configurationOptions);
        }

        /// <summary>
        /// Adds the type used to configure the logging system and create instances of <see cref="ILogger"/>
        /// </summary>
        /// <param name="loggerFactory"></param>
        public void AddLoggerFactory(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        /// <summary>
        /// Adds a custom subscriber.
        /// </summary>
        /// <param name="subscriberFactory">The delegate used to create the custom implementation of <see cref="ISubscriber"/>.</param>
        public void AddSubscriber(Func<ISubscriber> subscriberFactory)
        {
            if (CreateSubscriber != null)
            {
                throw new InvalidOperationException("A subscriber has already been configured.");
            }

            CreateSubscriber = subscriberFactory ?? throw new ArgumentNullException(nameof(subscriberFactory));
        }

        /// <summary>
        /// Adds RabbitMQ as the configuration subscriber.
        /// </summary>
        /// <param name="configure">Configure options for the RabbitMQ subscriber.</param>
        public void AddRabbitMqSubscriber(Action<RabbitMqOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (CreateSubscriber != null)
            {
                throw new InvalidOperationException("A subscriber has already been configured.");
            }

            var options = new RabbitMqOptions();
            configure(options);

            CreateSubscriber = () => new RabbitMqSubscriber(options);
        }

        /// <summary>
        /// Adds Redis as the configuration subscriber.
        /// </summary>
        /// <param name="configure">Configure options for the Redis multiplexer.</param>
        public void AddRedisSubscriber(Action<RedisOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (CreateSubscriber != null)
            {
                throw new InvalidOperationException("A subscriber has already been configured.");
            }

            var options = new RedisOptions();
            configure(options);

            CreateSubscriber = () => new RedisSubscriber(options);
        }

        /// <summary>
        /// Adds Redis as the configuration subscriber.
        /// </summary>
        /// <param name="configuration">The string configuration for the Redis multiplexer.</param>
        public void AddRedisSubscriber(string configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (CreateSubscriber != null)
            {
                throw new InvalidOperationException("A subscriber has already been configured.");
            }

            var options = RedisOptions.Parse(configuration);

            CreateSubscriber = () => new RedisSubscriber(options);
        }

        /// <summary>
        /// Adds NATS as the configuration subscriber.
        /// </summary>
        /// <param name="configure">Configure options for the NATS connection.</param>
        public void AddNatsSubscriber(Action<NatsOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            if (CreateSubscriber != null)
            {
                throw new InvalidOperationException("A subscriber has already been configured.");
            }

            var options = ConnectionFactory.GetDefaultOptions();
            configure(options);

            CreateSubscriber = () => new NatsSubscriber(options);
        }
    }
}