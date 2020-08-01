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
        /// <param name="configure">Configures the source.</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddRemoteConfiguration(this IConfigurationBuilder builder, Action<RemoteConfigurationOptions> configure)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new RemoteConfigurationOptions();
            configure(options);

            var remoteBuilder = new RemoteConfigurationBuilder(builder, options);
            return remoteBuilder;
        }
    }
}