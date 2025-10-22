using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MeAjudaAi.Shared.Common.Constants;
using MeAjudaAi.Shared.Messaging.Factory;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Strategy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Routing;
using Rebus.Routing.TypeBased;
using Rebus.Transport;

namespace MeAjudaAi.Shared.Messaging;

internal static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<MessageBusOptions>? configureOptions = null)
    {
        // Verifica se o messaging está habilitado
        var isEnabled = configuration.GetValue<bool>("Messaging:Enabled", true);
        if (!isEnabled)
        {
            // Registra um message bus no-op se o messaging estiver desabilitado
            services.AddSingleton<IMessageBus, NoOpMessageBus>();
            return services;
        }

        // Registro direto das configurações do Service Bus
        services.AddSingleton(provider =>
        {
            var options = new ServiceBusOptions();
            ConfigureServiceBusOptions(options, configuration);

            // Validações manuais com mensagens claras
            if (string.IsNullOrWhiteSpace(options.DefaultTopicName))
                throw new InvalidOperationException("ServiceBus DefaultTopicName is required when messaging is enabled. Configure 'Messaging:ServiceBus:DefaultTopicName' in appsettings.json");

            // Validação mais rigorosa da connection string
            if (string.IsNullOrWhiteSpace(options.ConnectionString) ||
                options.ConnectionString.Contains("${", StringComparison.OrdinalIgnoreCase) || // Check for unresolved environment variable placeholder
                options.ConnectionString.Equals("Endpoint=sb://localhost/;SharedAccessKeyName=default;SharedAccessKey=default", StringComparison.OrdinalIgnoreCase)) // Check for dummy connection string
            {
                if (environment.IsDevelopment() || environment.IsEnvironment(EnvironmentNames.Testing))
                {
                    // Para desenvolvimento/teste, log warning mas permita continuar
                    var logger = provider.GetService<Microsoft.Extensions.Logging.ILogger<ServiceBusOptions>>();
                    logger?.LogWarning("ServiceBus connection string is not configured. Messaging functionality will be limited in {Environment} environment.", environment.EnvironmentName);
                }
                else
                {
                    throw new InvalidOperationException($"ServiceBus connection string is required for {environment.EnvironmentName} environment. " +
                        "Set the SERVICEBUS_CONNECTION_STRING environment variable or configure 'Messaging:ServiceBus:ConnectionString' in appsettings.json. " +
                        "If messaging is not needed, set 'Messaging:Enabled' to false.");
                }
            }

            return options;
        });

        // Registro direto das configurações do RabbitMQ
        services.AddSingleton(provider =>
        {
            var options = new RabbitMqOptions();
            ConfigureRabbitMqOptions(options, configuration);

            // Validação manual
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException("RabbitMQ connection string not found. Ensure Aspire rabbitmq connection is available or configure 'Messaging:RabbitMQ:ConnectionString' in appsettings.json");

            return options;
        });

        // Registro direto das configurações do MessageBus
        services.AddSingleton(provider =>
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

        // Registrar implementações específicas do MessageBus condicionalmente baseado no ambiente
        // para reduzir o risco de resolução acidental em ambientes de teste
        if (environment.IsDevelopment())
        {
            // Development: Registra RabbitMQ e NoOp (fallback)
            services.TryAddSingleton<RabbitMqMessageBus>();
            services.TryAddSingleton<NoOp.NoOpMessageBus>();
        }
        else if (environment.IsProduction())
        {
            // Production: Registra apenas ServiceBus
            services.TryAddSingleton<ServiceBusMessageBus>();
        }
        else if (environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // Testing: Registra apenas NoOp - mocks serão adicionados via AddMessagingMocks()
            services.TryAddSingleton<NoOp.NoOpMessageBus>();
        }
        else
        {
            // Ambiente desconhecido: Registra todas as implementações para compatibilidade
            services.TryAddSingleton<ServiceBusMessageBus>();
            services.TryAddSingleton<RabbitMqMessageBus>();
            services.TryAddSingleton<NoOp.NoOpMessageBus>();
        }

        // Registrar o factory e o IMessageBus baseado no ambiente
        services.AddSingleton<IMessageBusFactory, EnvironmentBasedMessageBusFactory>();
        services.AddSingleton(serviceProvider =>
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

        // Adicionar sistema de Dead Letter Queue
        MeAjudaAi.Shared.Messaging.Extensions.DeadLetterExtensions.AddDeadLetterQueue(services, configuration);

        // TODO: Reabilitar após configurar Rebus v3
        // Rebus configuration temporariamente desabilitada

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
    /// Garante a infraestrutura de messaging para o transporte apropriado (RabbitMQ em dev, Azure Service Bus em prod)
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

        // Garantir infraestrutura de Dead Letter Queue
        await MeAjudaAi.Shared.Messaging.Extensions.DeadLetterExtensions.EnsureDeadLetterInfrastructureAsync(host);

        // Validar configuração de Dead Letter Queue
        await MeAjudaAi.Shared.Messaging.Extensions.DeadLetterExtensions.ValidateDeadLetterConfigurationAsync(host);
    }

    private static void ConfigureServiceBusOptions(ServiceBusOptions options, IConfiguration configuration)
    {
        configuration.GetSection(ServiceBusOptions.SectionName).Bind(options);

        // Tenta obter a connection string do Aspire primeiro
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            options.ConnectionString = configuration.GetConnectionString("servicebus") ?? string.Empty;
        }

        // Para ambientes de desenvolvimento/teste, fornece valores padrão mesmo sem connection string
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        if (environment == "Development" || environment == "Testing")
        {
            // Fornece padrões para desenvolvimento para evitar problemas de injeção de dependência
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
        // Tenta obter a connection string do Aspire primeiro
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            options.ConnectionString = configuration.GetConnectionString("rabbitmq") ?? options.BuildConnectionString();
        }
    }
}
