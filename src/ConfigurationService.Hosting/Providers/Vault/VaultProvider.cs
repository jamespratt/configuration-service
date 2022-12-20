using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VaultSharp;

namespace ConfigurationService.Hosting.Providers.Vault
{
    public class VaultProvider : IProvider
    {
        private readonly ILogger<VaultProvider> _logger;

        private readonly VaultProviderOptions _providerOptions;

        private IVaultClient _vaultClient;
        private readonly IDictionary<string, int> _secretVersions = new Dictionary<string, int>();

        public string Name => "Vault";

        public VaultProvider(ILogger<VaultProvider> logger, VaultProviderOptions providerOptions)
        {
            _logger = logger;
            _providerOptions = providerOptions;

            if (string.IsNullOrWhiteSpace(_providerOptions.ServerUri))
            {
                throw new ProviderOptionNullException(nameof(_providerOptions.ServerUri));
            }

            if (string.IsNullOrWhiteSpace(_providerOptions.Path))
            {
                throw new ProviderOptionNullException(nameof(_providerOptions.Path));
            }

            if (_providerOptions.AuthMethodInfo == null)
            {
                throw new ProviderOptionNullException(nameof(_providerOptions.AuthMethodInfo));
            }
        }

        public async Task Watch(Func<IEnumerable<string>, Task> onChange, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var changes = new List<string>();

                    var paths = await ListPaths();

                    foreach (var path in paths)
                    {
                        var metadata = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretMetadataAsync(path, _providerOptions.Path);

                        _secretVersions.TryGetValue(path, out int version);

                        if (version != metadata.Data.CurrentVersion)
                        {
                            changes.Add(path);

                            _secretVersions[path] = metadata.Data.CurrentVersion;
                        }
                    }

                    if (changes.Count > 0)
                    {
                        await onChange(changes);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred while attempting to poll for changes");
                }

                var delayDate = DateTime.UtcNow.Add(_providerOptions.PollingInterval);

                _logger.LogInformation("Next polling period will begin in {PollingInterval:c} at {DelayDate}",
                    _providerOptions.PollingInterval, delayDate);

                await Task.Delay(_providerOptions.PollingInterval, cancellationToken);
            }
        }

        public void Initialize()
        {
            _logger.LogInformation("Initializing {Name} provider with options {@Options}", Name, new
            {
                _providerOptions.ServerUri,
                _providerOptions.Path
            });

            var vaultClientSettings = new VaultClientSettings(_providerOptions.ServerUri, _providerOptions.AuthMethodInfo);

            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<byte[]> GetConfiguration(string name)
        {
            var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(name, null, _providerOptions.Path);

            if (secret == null)
            {
                _logger.LogInformation("Secret does not exist at {Name}", name);
                return null;
            }

            await using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, secret.Data.Data);
            return stream.ToArray();
        }

        public async Task<string> GetHash(string name)
        {
            var bytes = await GetConfiguration(name);

            return Hasher.CreateHash(bytes);
        }

        public async Task<IEnumerable<string>> ListPaths()
        {
            _logger.LogInformation("Listing paths at {Path}", _providerOptions.Path);

            var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync(null, _providerOptions.Path);
            var paths = secret.Data.Keys.ToList();

            _logger.LogInformation("{Count} paths found", paths.Count);

            return paths;
        }
    }
}