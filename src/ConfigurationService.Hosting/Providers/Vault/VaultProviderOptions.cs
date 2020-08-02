using System;
using VaultSharp.V1.AuthMethods;

namespace ConfigurationService.Hosting.Providers.Vault
{
    /// <summary>
    /// Options for <see cref="VaultProvider"/>.
    /// </summary>
    public class VaultProviderOptions
    {
        /// <summary>
        /// The Vault Server Uri with port.
        /// </summary>
        public string ServerUri { get; set; }

        /// <summary>
        /// The path where the kv secrets engine is enabled.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The auth method to be used to acquire a vault token.
        /// </summary>
        public IAuthMethodInfo AuthMethodInfo { get; set; }

        /// <summary>
        /// The interval to check for for remote changes. Defaults to 60 seconds.
        /// </summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(60);
    }
}