using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConfigurationService.Client
{
    public static class RemoteConfigurationExtensions
    {
        /// <summary>
        /// Adds a remote configuration source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configurationName">Short name of the configuration file relative to the configuration provider.</param>
        /// <param name="configurationServiceUri">Configuration service endpoint.</param>
        /// <param name="subscriberConfiguration">Connection string for the subscriber.</param>
        /// <param name="optional">Determines if loading the file is optional.</param>
        /// <param name="reloadOnChange">Determines whether the source will be loaded if the underlying file changes.</param>
        /// <param name="loggerFactory">The type used to configure the logging system and create instances of <see cref="ILogger"/>.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
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

            return builder.AddRemoteSource(s => 
            {
                s.ConfigurationServiceUri = configurationServiceUri;
                s.ConfigurationName = configurationName;
                s.Optional = optional;
                s.ReloadOnChange = reloadOnChange;
                s.SubscriberConfiguration = subscriberConfiguration;
                s.LoggerFactory = loggerFactory ?? new NullLoggerFactory();
            });
        }

        /// <summary>
        /// Adds a remote configuration source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRemoteSource(this IConfigurationBuilder builder, Action<RemoteConfigurationSource> configureSource)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureSource == null)
            {
                throw new ArgumentNullException(nameof(configureSource));
            }

            return builder.Add(configureSource);
        }
    }
}