using ConfigurationService.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfigurationService()
    .AddGitProvider(c =>
    {
        c.RepositoryUrl = "https://github.com/jamespratt/configuration-test.git";
        c.LocalPath = "C:/local-repo";
    })
    .AddRedisPublisher("localhost:6379");

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapConfigurationService();
});

app.Run();