using System;
using System.Collections.Generic;
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

        public string Name => "Vault";

        public VaultProvider(ILogger<VaultProvider> logger, VaultProviderOptions providerOptions)
        {
            _logger = logger;
            _providerOptions = providerOptions;

            if (string.IsNullOrWhiteSpace(_providerOptions.Path))
            {
                throw new ArgumentNullException(nameof(_providerOptions.Path), $"{nameof(_providerOptions.Path)} cannot be NULL or empty.");
            }
        }

        public Task Watch(Func<IEnumerable<string>, Task> onChange, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Initialize()
        {
            _logger.LogInformation("Initializing {Name} provider with options {Options}.", Name, new
            {
                _providerOptions.ServerUri,
                _providerOptions.Path
            });

            var vaultClientSettings = new VaultClientSettings(_providerOptions.ServerUri, _providerOptions.AuthMethodInfo);

            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<byte[]> GetConfiguration(string name)
        {
            var secret = await _vaultClient.V1.Secrets.KeyValue.V1.ReadSecretAsync(name);

            if (secret == null)
            {
                _logger.LogInformation("Secret does not exist at {name}.", name);
                return null;
            }

            var bytes = JsonSerializer.SerializeToUtf8Bytes(secret);
            return bytes;
        }

        public async Task<string> GetHash(string name)
        {
            var bytes = await GetConfiguration(name);

            return Hasher.CreateHash(bytes);
        }

        public async Task<IEnumerable<string>> ListPaths()
        {
            _logger.LogInformation("Listing paths at {Path}.", _providerOptions.Path);

            var secret = await _vaultClient.V1.Secrets.KeyValue.V1.ReadSecretPathsAsync(_providerOptions.Path);
            var paths = secret.Data.Keys.ToList();

            _logger.LogInformation("{Count} paths found.", paths.Count);

            return paths;
        }
    }
}