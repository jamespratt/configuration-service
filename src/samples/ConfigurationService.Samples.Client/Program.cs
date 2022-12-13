using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConfigurationService.Client;
using ConfigurationService.Client.Parsers;

namespace ConfigurationService.Samples.Client
{
    class Program
    {
        static async Task Main()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            IConfiguration localConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var configuration = new ConfigurationBuilder()
                .AddConfiguration(localConfiguration)
                .AddRemoteConfiguration(o =>
                {
                    o.ServiceUri = "http://localhost:5000/configuration/";
                    o.AddConfiguration(c =>
                    {
                        c.ConfigurationName = "test.json";
                        c.ReloadOnChange = true;
                        c.Optional = false;
                    });
                    o.AddConfiguration(c =>
                    {
                        c.ConfigurationName = "test.yaml";
                        c.ReloadOnChange = true;
                        c.Optional = false;
                        c.Parser = new YamlConfigurationFileParser();
                    });
                    o.AddRedisSubscriber("localhost:6379");
                    o.AddLoggerFactory(loggerFactory);
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<ConfigWriter>();
            services.Configure<TestConfig>(configuration.GetSection("Config"));

            var serviceProvider = services.BuildServiceProvider();

            var configWriter = serviceProvider.GetService<ConfigWriter>();

            await configWriter.Write();
        }
    }
}