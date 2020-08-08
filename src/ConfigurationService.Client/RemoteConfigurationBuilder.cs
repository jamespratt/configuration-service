using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace ConfigurationService.Client
{
    internal class RemoteConfigurationBuilder : IConfigurationBuilder
    {
        private readonly IConfigurationBuilder _configurationBuilder;

        private readonly RemoteConfigurationOptions _remoteConfigurationOptions;

        public IDictionary<string, object> Properties => _configurationBuilder.Properties;

        public IList<IConfigurationSource> Sources => _configurationBuilder.Sources;

        public RemoteConfigurationBuilder(IConfigurationBuilder configurationBuilder, RemoteConfigurationOptions remoteConfigurationOptions)
        {
            _configurationBuilder = configurationBuilder ?? throw new ArgumentNullException(nameof(configurationBuilder));
            _remoteConfigurationOptions = remoteConfigurationOptions ?? throw new ArgumentNullException(nameof(remoteConfigurationOptions));
        }

        public IConfigurationBuilder Add(IConfigurationSource source)
        {
            return _configurationBuilder.Add(source);
        }

        public IConfigurationRoot Build()
        {
            foreach (var configuration in _remoteConfigurationOptions.Configurations)
            {
                var source = new RemoteConfigurationSource
                {
                    ConfigurationServiceUri = _remoteConfigurationOptions.ServiceUri,
                    HttpMessageHandler = _remoteConfigurationOptions.HttpMessageHandler,
                    RequestTimeout = _remoteConfigurationOptions.RequestTimeout,
                    LoggerFactory = _remoteConfigurationOptions.LoggerFactory,

                    ConfigurationName = configuration.ConfigurationName,
                    Optional = configuration.Optional,
                    ReloadOnChange = configuration.ReloadOnChange,
                    Parser = configuration.Parser,

                    CreateSubscriber = _remoteConfigurationOptions.CreateSubscriber
                };

                Add(source);
            }

            return _configurationBuilder.Build();
        }
    }
}