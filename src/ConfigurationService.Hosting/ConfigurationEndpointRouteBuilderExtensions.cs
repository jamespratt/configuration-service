using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using ConfigurationService.Hosting.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ConfigurationService.Hosting;

public static class ConfigurationEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapConfigurationService(this IEndpointRouteBuilder endpoints, string pattern = "/configuration")
    {
        if (endpoints == null)
        {
            throw new ArgumentNullException(nameof(endpoints));
        }

        if (pattern == null)
        {
            throw new ArgumentNullException(nameof(pattern));
        }

        var conventionBuilders = new List<IEndpointConventionBuilder>();

        var listConfigurationBuilder = endpoints.RegisterListRoute(pattern);
        conventionBuilders.Add(listConfigurationBuilder);

        var fileConfigurationBuilder = endpoints.RegisterFileRoute(pattern);
        conventionBuilders.Add(fileConfigurationBuilder);

        return new CompositeEndpointConventionBuilder(conventionBuilders);
    }

    private static IEndpointConventionBuilder RegisterListRoute(this IEndpointRouteBuilder endpointRouteBuilder, string pattern)
    {
        var provider = endpointRouteBuilder.ServiceProvider.GetService<IProvider>();

        return endpointRouteBuilder.MapGet(pattern, async context =>
        {
            var files = await provider.ListPaths();

            context.Response.OnStarting(async () =>
            {
                await JsonSerializer.SerializeAsync(context.Response.Body, files);
            });

            context.Response.ContentType = "application/json; charset=UTF-8";
            await context.Response.Body.FlushAsync();
        });
    }

    private static IEndpointConventionBuilder RegisterFileRoute(this IEndpointRouteBuilder endpointRouteBuilder, string pattern)
    {
        var provider = endpointRouteBuilder.ServiceProvider.GetService<IProvider>();

        return endpointRouteBuilder.MapGet(pattern + "/{name}", async context =>
        {
            var name = context.GetRouteValue("name")?.ToString();
            name = WebUtility.UrlDecode(name);

            var bytes = await provider.GetConfiguration(name);

            if (bytes == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            var fileContent = Encoding.UTF8.GetString(bytes);

            await context.Response.WriteAsync(fileContent);
            await context.Response.Body.FlushAsync();
        });
    }

    private sealed class CompositeEndpointConventionBuilder : IEndpointConventionBuilder
    {
        private readonly List<IEndpointConventionBuilder> _endpointConventionBuilders;

        public CompositeEndpointConventionBuilder(List<IEndpointConventionBuilder> endpointConventionBuilders)
        {
            _endpointConventionBuilders = endpointConventionBuilders;
        }

        public void Add(Action<EndpointBuilder> convention)
        {
            foreach (var endpointConventionBuilder in _endpointConventionBuilders)
            {
                endpointConventionBuilder.Add(convention);
            }
        }
    }
}