using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConfigurationService.Client
{
    public static class RemoteConfigurationExtensions
    {
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, string configurationName, string configurationServiceUri, 
            string subscriberConfiguration = null, bool optional = false, bool reloadOnChange = false, ILoggerFactory loggerFactory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configurationName == null)
            {
                throw new ArgumentNullException(nameof(configurationName));
            }

            if (configurationServiceUri == null)
            {
                throw new ArgumentNullException(nameof(configurationServiceUri));
            }

            if (reloadOnChange && subscriberConfiguration == null)
            {
                throw new ArgumentNullException(nameof(subscriberConfiguration), $"Value cannot be null if {nameof(reloadOnChange)} is true.");
            }

            var source = new RemoteConfigurationSource
            {
                ConfigurationServiceUri = configurationServiceUri,
                ConfigurationName = configurationName,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
                SubscriberConfiguration = subscriberConfiguration,
                LoggerFactory = loggerFactory ?? new NullLoggerFactory()
            };

            return builder.AddRemoteSource(source);
        }

        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, RemoteConfigurationSource source)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            builder.Add(source);
            return builder;
        }
    }
}