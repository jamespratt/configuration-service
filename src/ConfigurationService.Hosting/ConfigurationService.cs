using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationService.Hosting.Providers;
using ConfigurationService.Hosting.Publishers;
using Microsoft.Extensions.Logging;

namespace ConfigurationService.Hosting
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILogger<ConfigurationService> _logger;

        private readonly IProvider _provider;
        private readonly IPublisher _publisher;

        public ConfigurationService(ILogger<ConfigurationService> logger, IProvider provider, IPublisher publisher = null)
        {
            _logger = logger;
            _provider = provider;
            _publisher = publisher;

            if (_publisher == null)
            {
                _logger.LogInformation("A publisher has not been configured.");
            }
        }

        public async Task InitializeProvider(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initializing {Name} configuration provider...", _provider.Name);

            _provider.Initialize();

            var files = _provider.ListAllFiles();

            await PublishChanges(files);

            await _provider.Watch(OnChange, cancellationToken);

            _logger.LogInformation("{Name} configuration watching for changes.", _provider.Name);
        }

        public async Task OnChange(IEnumerable<string> files)
        {
            _logger.LogInformation("Changes were detected on the remote {Name} configuration provider.", _provider.Name);

            files = files.ToList();

            if (files.Any())
            {
                await PublishChanges(files);
            }
        }

        public async Task PublishChanges(IEnumerable<string> files)
        {
            if (_publisher == null)
            {
                return;
            }

            _logger.LogInformation("Publishing changes...");

            foreach (var filePath in files)
            {
                var hash = await _provider.GetHash(filePath);
                await _publisher.Publish(filePath, hash);
            }
        }
    }
}