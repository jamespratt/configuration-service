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