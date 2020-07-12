# ConfigurationService

## About ConfigurationService

ConfigurationService is an externalized configuration service for .NET Core.  Configuration for fleets of applications, services, and containerized micro-services can be updated immediately without the need to redeploy or restart. ConfigurationService uses a client/server pub/sub architecture to notify subscribed clients of configuration changes as they happen.  Configuration can be injected using the standard options pattern with `IOptions`, `IOptionsMonitor` or `IOptionsSnapshot`.

Configuration service currently supports hosting configuration with git and Redis pub/sub.  Additional providers and publishers will be added in future releases.

## Installing with NuGet

The easiest way to install ConfigurationService is with [NuGet](https://www.nuget.org/packages/ConfigurationService.Hosting/).

In Visual Studio's [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console),
enter the following command:

Hosting:

    Install-Package ConfigurationService.Hosting
    
Client:

    Install-Package ConfigurationService.Client
    
## Adding the ConfigurationService Host
The ConfigurationService host middleware can be added to the service collection of an existing ASP.NET Core application.  The following example configures a git storage provider with a Redis publisher.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    services.AddConfigurationService()
        .AddGitProvider(new GitProviderOptions
        {
            RepositoryUrl = "https://example.com/my-repo/configuration.git",
            Username = "username",
            Password = "password",
            LocalPath = "C:/config"
        })
        .AddRedisPublisher("localhost:6379");
}
```
The configured host will expose two API endpoints:
* `configuration/list` - Lists all files at the configured provider.
* `configuration/{filename}` - Retrieves the contents of the specified file.

## Adding the ConfigurationService Client
The ConfigurationService client can be configured by adding `AddRemoteSource` to a new or existing configuration builder. In the following example, remote json configuration is added and a Redis endpoint is specified for configuration change subscription.  Local configuration can be read for settings for the remote source by using multiple `Build` instances of the configuration. 

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

configuration = new ConfigurationBuilder()
    .AddConfiguration(configuration)
    .AddRemoteSource(new RemoteConfigurationSource
    {
        ConfigurationName = "test.json",
        ConfigurationServiceUri = "http://localhost:5000/configuration/",
        SubscriberConfiguration = "localhost:6379",
        Optional = false,
        ReloadOnChange = true,
        LoggerFactory = loggerFactory
    })
    .Build();
```

## Samples
Samples of both host and client implementations can be viewed at [Samples](https://github.com/jamespratt/configuration-service/tree/master/samples).
