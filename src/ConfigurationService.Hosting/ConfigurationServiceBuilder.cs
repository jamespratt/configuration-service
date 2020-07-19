using System;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationService.Hosting
{
    public class ConfigurationServiceBuilder : IConfigurationServiceBuilder
    {
        public IServiceCollection Services { get; }

        public ConfigurationServiceBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}