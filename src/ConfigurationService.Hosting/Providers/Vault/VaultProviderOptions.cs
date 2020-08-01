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
        /// The location path where the secret needs to be read from.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The auth method to be used to acquire a vault token.
        /// </summary>
        public IAuthMethodInfo AuthMethodInfo { get; set; }
    }
}