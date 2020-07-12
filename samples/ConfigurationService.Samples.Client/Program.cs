using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConfigurationService.Client;

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

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            configuration = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .AddRemoteSource(new RemoteConfigurationSource
                {
                    ConfigurationName = configuration["ConfigurationName"],
                    ConfigurationServiceUri = configuration["ConfigurationServiceUri"],
                    SubscriberConfiguration = configuration["SubscriberConfiguration"],
                    Optional = false,
                    ReloadOnChange = true,
                    LoggerFactory = loggerFactory
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<ConfigWriter>();
            services.Configure<TestConfig>(configuration.GetSection("Config"));

            var serviceProvider = services.BuildServiceProvider();

            var configWriter = serviceProvider.GetService<ConfigWriter>();

            var cts = new CancellationTokenSource();
            await configWriter.Write(cts.Token);
            cts.Dispose();
        }
    }
}