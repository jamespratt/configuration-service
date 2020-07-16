using System;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ConfigurationService.Providers;
using ConfigurationService.Providers.Git;
using ConfigurationService.Publishers;
using ConfigurationService.Publishers.Redis;

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
    }
}