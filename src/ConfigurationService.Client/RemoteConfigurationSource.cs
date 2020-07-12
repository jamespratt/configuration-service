using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConfigurationService.Client
{
    public class RemoteConfigurationSource : IConfigurationSource
    {
        public string ConfigurationName { get; set; }

        public string ConfigurationServiceUri { get; set; }

        public string SubscriberConfiguration { get; set; }

        public bool Optional { get; set; }

        public bool ReloadOnChange { get; set; }

        public HttpMessageHandler HttpMessageHandler { get; set; }

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(60);

        public IConfigurationParser Parser { get; set; }

        public ISubscriber Subscriber { get; set; }

        public ILoggerFactory LoggerFactory { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new RemoteConfigurationProvider(this);
        }
    }
}