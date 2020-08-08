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

        public async Task Initialize(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initializing {Name} configuration provider...", _provider.Name);

            _provider.Initialize();

            if (_publisher != null)
            {
                _logger.LogInformation("Initializing publisher...");
                _publisher.Initialize();
            }

            var paths = await _provider.ListPaths();

            await PublishChanges(paths);

            await _provider.Watch(OnChange, cancellationToken);

            _logger.LogInformation("{Name} configuration watching for changes.", _provider.Name);
        }

        public async Task OnChange(IEnumerable<string> paths)
        {
            _logger.LogInformation("Changes were detected on the remote {Name} configuration provider.", _provider.Name);

            paths = paths.ToList();

            if (paths.Any())
            {
                await PublishChanges(paths);
            }
        }

        public async Task PublishChanges(IEnumerable<string> paths)
        {
            if (_publisher == null)
            {
                return;
            }

            _logger.LogInformation("Publishing changes...");

            foreach (var path in paths)
            {
                var hash = await _provider.GetHash(path);
                await _publisher.Publish(path, hash);
            }
        }
    }
}