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
        /// <param name="providerOptions">The git provider options.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddGitProvider(this IConfigurationServiceBuilder builder, GitProviderOptions providerOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (providerOptions == null)
            {
                throw new ArgumentNullException(nameof(providerOptions));
            }

            builder.Services.AddSingleton<IProvider>(sp => ActivatorUtilities.CreateInstance<GitProvider>(sp, providerOptions));

            return builder;
        }

        /// <summary>
        /// Adds Redis as the configuration publisher.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationServiceBuilder"/> to add services to.</param>
        /// <param name="configurationOptions">The configuration options for the Redis multiplexer.</param>
        /// <returns>An <see cref="IConfigurationServiceBuilder"/> that can be used to further configure the 
        /// ConfigurationService services.</returns>
        public static IConfigurationServiceBuilder AddRedisPublisher(this IConfigurationServiceBuilder builder, ConfigurationOptions configurationOptions)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configurationOptions == null)
            {
                throw new ArgumentNullException(nameof(configurationOptions));
            }

            builder.Services.AddSingleton<IPublisher>(sp => ActivatorUtilities.CreateInstance<RedisPublisher>(sp, configurationOptions));

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

            builder.Services.AddSingleton<IPublisher>(sp => ActivatorUtilities.CreateInstance<RedisPublisher>(sp, configuration));

            return builder;
        }
    }
}