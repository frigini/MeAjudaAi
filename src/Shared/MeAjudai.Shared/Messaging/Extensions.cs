using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Strategy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        // Configure Azure Service Bus options
        services.AddOptions<ServiceBusOptions>()
            .Configure(opts => ConfigureServiceBusOptions(opts, configuration))
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.DefaultTopicName),
                "ServiceBus topic name not found. Configure 'Messaging:ServiceBus:TopicName' in appsettings.json")
            .Validate((opts, serviceProvider) => 
            {
                var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
                // Only require connection string in production
                return environment.IsDevelopment() || !string.IsNullOrWhiteSpace(opts.ConnectionString);
            }, "ServiceBus connection string not found. Configure 'Messaging:ServiceBus:ConnectionString' in appsettings.json or ensure Aspire servicebus connection is available")
            .ValidateOnStart();

        // Configure RabbitMQ options for development
        services.AddOptions<RabbitMqOptions>()
            .Configure(opts => ConfigureRabbitMqOptions(opts, configuration))
            .Validate(opts => !string.IsNullOrWhiteSpace(opts.ConnectionString),
                "RabbitMQ connection string not found. Ensure Aspire rabbitmq connection is available or configure 'Messaging:RabbitMQ:ConnectionString' in appsettings.json");

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
            var rabbitMqOptions = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var messageBusOptions = serviceProvider.GetRequiredService<IOptions<MessageBusOptions>>().Value;
            var eventRegistry = serviceProvider.GetRequiredService<IEventTypeRegistry>();
            var topicSelector = serviceProvider.GetRequiredService<ITopicStrategySelector>();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

            return configure
                .Transport(t => ConfigureTransport(t, serviceBusOptions, rabbitMqOptions, environment))
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

        services.AddSingleton(serviceProvider =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
            return new ServiceBusAdministrationClient(serviceBusOptions.ConnectionString);
        });

        services.AddSingleton<IServiceBusTopicManager, ServiceBusTopicManager>();
        services.AddSingleton<IRabbitMqInfrastructureManager, RabbitMqInfrastructureManager>();

        return services;
    }

    public static async Task EnsureServiceBusTopicsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var topicManager = scope.ServiceProvider.GetRequiredService<IServiceBusTopicManager>();
        await topicManager.EnsureTopicsExistAsync();
    }

    public static async Task EnsureRabbitMqInfrastructureAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var infrastructureManager = scope.ServiceProvider.GetRequiredService<IRabbitMqInfrastructureManager>();
        await infrastructureManager.EnsureInfrastructureAsync();
    }

    /// <summary>
    /// Ensures messaging infrastructure for the appropriate transport (RabbitMQ in dev, Azure Service Bus in prod)
    /// </summary>
    public static async Task EnsureMessagingInfrastructureAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        
        if (environment.IsDevelopment())
        {
            await host.EnsureRabbitMqInfrastructureAsync();
        }
        else
        {
            await host.EnsureServiceBusTopicsAsync();
        }
    }

    private static void ConfigureServiceBusOptions(ServiceBusOptions options, IConfiguration configuration)
    {
        configuration.GetSection(ServiceBusOptions.SectionName).Bind(options);
        // Try to get connection string from Aspire first
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            options.ConnectionString = configuration.GetConnectionString("servicebus") ?? string.Empty;
        }
    }

    private static void ConfigureRabbitMqOptions(RabbitMqOptions options, IConfiguration configuration)
    {
        configuration.GetSection(RabbitMqOptions.SectionName).Bind(options);
        // Try to get connection string from Aspire first
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            options.ConnectionString = configuration.GetConnectionString("rabbitmq") ?? options.BuildConnectionString();
        }
    }

    private static void ConfigureTransport(
        StandardConfigurer<ITransport> transport,
        ServiceBusOptions serviceBusOptions,
        RabbitMqOptions rabbitMqOptions,
        IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            transport.UseRabbitMq(
                rabbitMqOptions.ConnectionString,
                rabbitMqOptions.DefaultQueueName);
        }
        else
        {
            transport.UseAzureServiceBus(
                serviceBusOptions.ConnectionString,
                serviceBusOptions.DefaultTopicName);
        }
    }

    private async static Task ConfigureRoutingAsync(
        StandardConfigurer<IRouter> routing,
        IEventTypeRegistry eventRegistry,
        ITopicStrategySelector topicSelector)
    {
        var routingConfig = routing.TypeBased();
        var eventTypes = await eventRegistry.GetAllEventTypesAsync();

        foreach (var eventType in eventTypes)
        {
            var topicName = topicSelector.SelectTopicForEvent(eventType);
            routingConfig.Map(eventType, topicName);
        }
    }
}