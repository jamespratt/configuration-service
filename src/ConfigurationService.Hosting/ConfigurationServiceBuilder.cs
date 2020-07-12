using System;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationService.Hosting
{
    public class ConfigurationServiceBuilder : IConfigurationServiceBuilder
    {
        public IServiceCollection Services { get; }

        public ConfigurationServiceBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            Services = services;
        }
    }
}