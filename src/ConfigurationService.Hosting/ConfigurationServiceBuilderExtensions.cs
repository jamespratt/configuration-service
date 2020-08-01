using System;
using ConfigurationService.Hosting.Providers;
using ConfigurationService.Hosting.Providers.FileSystem;
using ConfigurationService.Hosting.Providers.Git;
using ConfigurationService.Hosting.Publishers;
using ConfigurationService.Hosting.Publishers.Nats;
using ConfigurationService.Hosting.Publishers.RabbitMq;
using ConfigurationService.Hosting.Publishers.Redis;
using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using StackExchange.Redis;

namespace ConfigurationService.Hosting
{
    public static class ConfigurationServiceBuilderExtensions
    {
        /// <summary>
        /// Adds services for configuration hosting to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddConfigurationService(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHostedService<HostedConfigurationService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            return new ConfigurationServiceBuilder(services);
        }

        /// <summary>
        /// Add Git as the storage provider backend.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configure">Configure git provider options.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddGitProvider(this IConfigurationServiceBuilder builder, Action<GitProviderOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new GitProviderOptions();
            configure(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IProvider, GitProvider>();

            return builder;
        }

        /// <summary>
        /// Add file system as the storage provider backend.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configure">Configure file system provider options.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddFileSystemProvider(this IConfigurationServiceBuilder builder, Action<FileSystemProviderOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new FileSystemProviderOptions();
            configure(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IProvider, FileSystemProvider>();

            return builder;
        }

        /// <summary>
        /// Adds a custom storage provider backend.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="provider">The custom implementation of <see cref="IProvider"/>.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddProvider(this IConfigurationServiceBuilder builder, IProvider provider)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            builder.Services.AddSingleton(provider);

            return builder;
        }

        /// <summary>
        /// Adds RabbitMQ as the configuration publisher.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configure">Configure options for the RabbitMQ publisher.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddRabbitMqPublisher(this IConfigurationServiceBuilder builder, Action<RabbitMqOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new RabbitMqOptions();
            configure(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IPublisher, RabbitMqPublisher>();

            return builder;
        }

        /// <summary>
        /// Adds Redis as the configuration publisher.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configure">Configure options for the Redis multiplexer.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddRedisPublisher(this IConfigurationServiceBuilder builder, Action<ConfigurationOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new ConfigurationOptions();
            configure(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IPublisher, RedisPublisher>();

            return builder;
        }

        /// <summary>
        /// Adds Redis as the configuration publisher.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configuration">The string configuration for the Redis multiplexer.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddRedisPublisher(this IConfigurationServiceBuilder builder, string configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var options = ConfigurationOptions.Parse(configuration);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IPublisher, RedisPublisher>();

            return builder;
        }

        /// <summary>
        /// Adds NATS as the configuration publisher.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configure">Configure options for the NATS connection.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddNatsPublisher(this IConfigurationServiceBuilder builder, Action<Options> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = ConnectionFactory.GetDefaultOptions();
            configure(options);

            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IPublisher, NatsPublisher>();

            return builder;
        }

        /// <summary>
        /// Adds a custom configuration publisher.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="publisher">The custom implementation of <see cref="IPublisher"/>.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddPublisher(this IConfigurationServiceBuilder builder, IPublisher publisher)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (publisher == null)
            {
                throw new ArgumentNullException(nameof(publisher));
            }

            builder.Services.AddSingleton(publisher);

            return builder;
        }
    }
}