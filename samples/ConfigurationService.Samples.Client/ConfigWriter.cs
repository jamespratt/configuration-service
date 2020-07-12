using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ConfigurationService.Samples.Client
{
    public class ConfigWriter
    {
        private readonly IOptionsMonitor<TestConfig> _testConfig;

        public ConfigWriter(IOptionsMonitor<TestConfig> testConfig)
        {
            _testConfig = testConfig;
        }

        public async Task Write(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var config = _testConfig.CurrentValue;
                Console.WriteLine(config.Text);

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }
}