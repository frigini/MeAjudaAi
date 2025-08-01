using Azure.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Routing;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Rebus.Transport;
using System.Text.Json;

namespace MeAjudaAi.Shared.Messaging;

internal static class Extensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MessageBusOptions>? configureOptions = null)
    {
        services.AddOptions<ServiceBusOptions>()
            .Configure(opts => configuration.GetSection(ServiceBusOptions.SectionName).Bind(opts))
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.ConnectionString),
                "ServiceBus connection string not found. Configure 'Messaging:ServiceBus:ConnectionString' in appsettings.json")
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.DefaultTopicName),
                "ServiceBus topic name not found. Configure 'Messaging:ServiceBus:TopicName' in appsettings.json");

        services.Configure<MessageBusOptions>(_ => { });
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.AddSingleton(serviceProvider =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            return new ServiceBusClient(serviceBusOptions.ConnectionString);
        });

        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();
        services.AddSingleton<ITopicStrategySelector, TopicStrategySelector>();

        services.AddRebus((configure, serviceProvider) =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            var messageBusOptions = serviceProvider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
            var eventRegistry = serviceProvider.GetRequiredService<IEventTypeRegistry>();
            var topicSelector = serviceProvider.GetRequiredService<ITopicStrategySelector>();

            return configure
                .Transport(t => ConfigureTransport(t, serviceBusOptions))
                .Routing(async r => await ConfigureRoutingAsync(r, eventRegistry, topicSelector))
                .Options(o =>
                {
                    o.SetNumberOfWorkers(messageBusOptions.MaxConcurrentCalls);
                    o.SetMaxParallelism(messageBusOptions.MaxConcurrentCalls);
                })
                .Serialization(s => s.UseSystemTextJson(new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
        });

        services.AddSingleton<IMessageBus, ServiceBusMessageBus>();

        return services;
    }

    private static void ConfigureTransport(
        StandardConfigurer<ITransport> transport,
        ServiceBusOptions serviceBusOptions)
    {
        transport.UseAzureServiceBus(
            serviceBusOptions.ConnectionString,
            serviceBusOptions.DefaultTopicName);
    }

    private async static Task ConfigureRoutingAsync(
        StandardConfigurer<IRouter> routing,
        IEventTypeRegistry eventRegistry,
        ITopicStrategySelector topicSelector)
    {
        var routingConfig = routing.TypeBased();

        foreach (var eventType in await eventRegistry.GetAllEventTypesAsync())
        {
            var topicName = topicSelector.SelectTopicForEvent(eventType);
            routingConfig.Map(eventType, topicName);
        }
    }
}