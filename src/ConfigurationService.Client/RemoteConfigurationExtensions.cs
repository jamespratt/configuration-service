using System;
using Microsoft.Extensions.Configuration;

namespace ConfigurationService.Client
{
    public static class RemoteConfigurationExtensions
    {
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