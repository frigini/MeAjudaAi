using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MeAjudaAi.Shared.Messaging.Factory;
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
        // Check if messaging is enabled
        var isEnabled = configuration.GetValue<bool>("Messaging:Enabled", true);
        if (!isEnabled)
        {
            // Register a no-op message bus if messaging is disabled
            services.AddSingleton<IMessageBus, NoOpMessageBus>();
            return services;
        }

        // Registro direto das configurações do Service Bus
        services.AddSingleton<ServiceBusOptions>(provider =>
        {
            var options = new ServiceBusOptions();
            ConfigureServiceBusOptions(options, configuration);
            
            // Validações manuais
            if (string.IsNullOrWhiteSpace(options.DefaultTopicName))
                throw new InvalidOperationException("ServiceBus topic name not found. Configure 'Messaging:ServiceBus:TopicName' in appsettings.json");
                
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            if (environment != "Development" && environment != "Testing" && string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException("ServiceBus connection string not found. Configure 'Messaging:ServiceBus:ConnectionString' in appsettings.json or ensure Aspire servicebus connection is available");
                
            return options;
        });

        // Registro direto das configurações do RabbitMQ
        services.AddSingleton<RabbitMqOptions>(provider =>
        {
            var options = new RabbitMqOptions();
            ConfigureRabbitMqOptions(options, configuration);
            
            // Validação manual
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException("RabbitMQ connection string not found. Ensure Aspire rabbitmq connection is available or configure 'Messaging:RabbitMQ:ConnectionString' in appsettings.json");
                
            return options;
        });

        // Registro direto das configurações do MessageBus
        services.AddSingleton<MessageBusOptions>(provider =>
        {
            var options = new MessageBusOptions();
            configureOptions?.Invoke(options);
            return options;
        });

        services.AddSingleton(serviceProvider =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<ServiceBusOptions>();
            return new ServiceBusClient(serviceBusOptions.ConnectionString);
        });

        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();
        services.AddSingleton<ITopicStrategySelector, TopicStrategySelector>();

        // Registrar implementações específicas do MessageBus
        services.AddSingleton<ServiceBusMessageBus>();
        services.AddSingleton<RabbitMqMessageBus>();
        services.AddSingleton<NoOp.NoOpMessageBus>();
        
        // Registrar o factory e o IMessageBus baseado no ambiente
        services.AddSingleton<IMessageBusFactory, EnvironmentBasedMessageBusFactory>();
        services.AddSingleton<IMessageBus>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<IMessageBusFactory>();
            return factory.CreateMessageBus();
        });

        services.AddSingleton(serviceProvider =>
        {
            var serviceBusOptions = serviceProvider.GetRequiredService<ServiceBusOptions>();
            return new ServiceBusAdministrationClient(serviceBusOptions.ConnectionString);
        });

        services.AddSingleton<IServiceBusTopicManager, ServiceBusTopicManager>();
        services.AddSingleton<IRabbitMqInfrastructureManager, RabbitMqInfrastructureManager>();

        // Only configure Rebus if not in Testing environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        if (environment != "Testing")
        {
            services.AddRebus((configure, serviceProvider) =>
            {
                var serviceBusOptions = serviceProvider.GetRequiredService<ServiceBusOptions>();
                var rabbitMqOptions = serviceProvider.GetRequiredService<RabbitMqOptions>();
                var messageBusOptions = serviceProvider.GetRequiredService<MessageBusOptions>();
                var eventRegistry = serviceProvider.GetRequiredService<IEventTypeRegistry>();
                var topicSelector = serviceProvider.GetRequiredService<ITopicStrategySelector>();
                var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();

                return configure
                    .Transport(t => ConfigureTransport(t, serviceBusOptions, rabbitMqOptions, hostEnvironment))
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
        }

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
        
        // For development/testing environments, provide default values even if no connection string
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        if (environment == "Development" || environment == "Testing")
        {
            // Provide defaults for development to avoid dependency injection issues
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                options.ConnectionString = "Endpoint=sb://localhost/;SharedAccessKeyName=default;SharedAccessKey=default";
            }
            
            if (string.IsNullOrWhiteSpace(options.DefaultTopicName))
            {
                options.DefaultTopicName = "MeAjudaAi-events";
            }
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
        if (environment.EnvironmentName == "Testing")
        {
            // For testing, use RabbitMQ with minimal configuration
            // This will fail gracefully and not block the application startup
            transport.UseRabbitMq("amqp://localhost", "test-queue");
        }
        else if (environment.IsDevelopment())
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