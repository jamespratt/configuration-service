using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationService.Hosting
{
    public interface IConfigurationServiceBuilder
    {
        IServiceCollection Services { get; }
    }
}