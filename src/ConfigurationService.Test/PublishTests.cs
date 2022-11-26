using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConfigurationService.Hosting;
using ConfigurationService.Hosting.Providers;
using ConfigurationService.Hosting.Publishers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ConfigurationService.Test
{
    public class PublishTests
    {
        private readonly ILogger<Hosting.ConfigurationService> _logger;
        private readonly IProvider _provider;
        private readonly IPublisher _publisher;
        private readonly IConfigurationService _configurationService;

        public PublishTests()
        {
            _logger = Substitute.For<ILogger<Hosting.ConfigurationService>>();
            _publisher = Substitute.For<IPublisher>();
            _provider = SetupStorageProvider();
            _configurationService = new Hosting.ConfigurationService(_logger, _provider, _publisher);
        }

        [Fact]
        public async Task Publish_Invoked_on_Initialization()
        {
            await _configurationService.Initialize();

            await _publisher.Received().Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Publish_Invoked_on_Change()
        {
            await _configurationService.OnChange(ListRandomFiles(1));

            await _publisher.Received(1).Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Publish_Invoked_on_PublishChanges()
        {
            await _configurationService.PublishChanges(ListRandomFiles(1));

            await _publisher.Received(1).Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Publish_Invoked_on_Change_for_Each_File()
        {
            var fileCount = 5;
            await _configurationService.OnChange(ListRandomFiles(fileCount));

            await _publisher.Received(fileCount).Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Publish_Is_Not_Invoked_when_No_Change()
        {
            await _configurationService.OnChange(new List<string>());

            await _publisher.DidNotReceive().Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Publish_Does_Not_Fail_when_No_Publisher_Registered()
        {
            var configurationService = new Hosting.ConfigurationService(_logger, _provider);
            await configurationService.PublishChanges(ListRandomFiles(1));
            
            await _publisher.DidNotReceive().Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Publish_Is_Not_Invoked_when_No_Publisher_Registered()
        {
            var configurationService = new Hosting.ConfigurationService(_logger, _provider);
            await configurationService.OnChange(ListRandomFiles(1));

            await _publisher.DidNotReceive().Publish(Arg.Any<string>(), Arg.Any<string>());
        }

        private IProvider SetupStorageProvider()
        {
            var storageProvider = Substitute.For<IProvider>();
            storageProvider.ListPaths().Returns(ListRandomFiles(1));
            storageProvider.GetConfiguration(Arg.Any<string>()).Returns(name => Encoding.UTF8.GetBytes($"{{ \"name\": \"{name}\" }}"));
            return storageProvider;
        }

        private static IEnumerable<string> ListRandomFiles(int count)
        {
            var list = new List<string>();

            for (int i = 0; i < count; i++)
            {
                list.Add($"{Guid.NewGuid()}.json");
            }

            return list;
        }
    }
}