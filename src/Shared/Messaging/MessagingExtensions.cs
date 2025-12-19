using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using MeAjudaAi.Shared.Messaging.Strategy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging;

/// <summary>
/// Extension methods consolidados para configuração de Messaging, Dead Letter Queue e Message Retry
/// </summary>
public static class MessagingExtensions
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
        services.AddSingleton<IMessageBusFactory, MessageBusFactory>();
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
        services.AddDeadLetterQueue(configuration);

        // TODO(#248): Re-enable after Rebus v3 migration completes.
        // Blockers: (1) Rebus.ServiceProvider v10+ required for .NET 10 compatibility,
        // (2) Breaking changes in IHandleMessages<T> interface signatures,
        // (3) RebusConfigurer fluent API changes require ConfigureRebus() refactor.
        // Timeline: Planned for Sprint 5 after stabilizing current MassTransit/RabbitMQ integration.
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
        await host.EnsureDeadLetterInfrastructureAsync();

        // Validar configuração de Dead Letter Queue
        await host.ValidateDeadLetterConfigurationAsync();
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

    #region Dead Letter Queue Extensions

    /// <summary>
    /// Adiciona o sistema de Dead Letter Queue ao container de dependências
    /// </summary>
    public static IServiceCollection AddDeadLetterQueue(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DeadLetterOptions>? configureOptions = null)
    {
        // Configurar opções
        services.Configure<DeadLetterOptions>(configuration.GetSection(DeadLetterOptions.SectionName));

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Registrar implementações específicas
        services.AddScoped<RabbitMqDeadLetterService>();
        services.AddScoped<ServiceBusDeadLetterService>();
        services.AddScoped<NoOpDeadLetterService>();

        // Registrar factory
        services.AddScoped<Factories.IDeadLetterServiceFactory, Factories.DeadLetterServiceFactory>();

        // Registrar serviço principal baseado no ambiente
        services.AddScoped<IDeadLetterService>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<Factories.IDeadLetterServiceFactory>();
            return factory.CreateDeadLetterService();
        });

        // Adicionar middleware de retry
        services.AddMessageRetryMiddleware();

        return services;
    }

    /// <summary>
    /// Configura dead letter queue específico para RabbitMQ
    /// </summary>
    public static IServiceCollection AddRabbitMqDeadLetterQueue(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DeadLetterOptions>? configureOptions = null)
    {
        services.Configure<DeadLetterOptions>(configuration.GetSection(DeadLetterOptions.SectionName));

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.AddScoped<IDeadLetterService, RabbitMqDeadLetterService>();
        services.AddMessageRetryMiddleware();

        return services;
    }

    /// <summary>
    /// Configura dead letter queue específico para Azure Service Bus
    /// </summary>
    public static IServiceCollection AddServiceBusDeadLetterQueue(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DeadLetterOptions>? configureOptions = null)
    {
        services.Configure<DeadLetterOptions>(configuration.GetSection(DeadLetterOptions.SectionName));

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.AddScoped<IDeadLetterService, ServiceBusDeadLetterService>();
        services.AddMessageRetryMiddleware();

        return services;
    }

    /// <summary>
    /// Valida a configuração do Dead Letter Queue na aplicação
    /// </summary>
    public static Task ValidateDeadLetterConfigurationAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        IDeadLetterService? deadLetterService = null;

        try
        {
            deadLetterService = scope.ServiceProvider.GetRequiredService<IDeadLetterService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDeadLetterService>>();

            // Teste básico para verificar se o serviço está configurado corretamente
            var testException = new InvalidOperationException("Test exception for DLQ validation");
            var shouldRetry = deadLetterService.ShouldRetry(testException, 1);
            var retryDelay = deadLetterService.CalculateRetryDelay(1);

            logger.LogInformation(
                "Dead Letter Queue validation completed. Service: {ServiceType}, ShouldRetry: {ShouldRetry}, RetryDelay: {RetryDelay}ms",
                deadLetterService.GetType().Name, shouldRetry, retryDelay.TotalMilliseconds);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDeadLetterService>>();
            logger.LogError(ex, "Failed to validate Dead Letter Queue configuration. Service: {ServiceType}",
                deadLetterService?.GetType().Name ?? "unknown");
            throw new InvalidOperationException(
                $"Dead Letter Queue validation failed for {deadLetterService?.GetType().Name ?? "unknown"}", ex);
        }
    }

    /// <summary>
    /// Garante que a infraestrutura de Dead Letter Queue está criada
    /// </summary>
    public static Task EnsureDeadLetterInfrastructureAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        try
        {
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostEnvironment>>();

            if (environment.IsDevelopment())
            {
                // Para RabbitMQ, a infraestrutura é criada dinamicamente quando necessário
                logger.LogInformation("Dead Letter infrastructure for RabbitMQ will be created dynamically");
            }
            else
            {
                // Para Service Bus, a infraestrutura também é criada dinamicamente
                // mas você poderia verificar se as filas existem aqui
                logger.LogInformation("Dead Letter infrastructure for Service Bus will be created dynamically");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostEnvironment>>();
            logger.LogError(ex, "Failed to ensure Dead Letter Queue infrastructure");
            throw new InvalidOperationException(
                "Failed to ensure Dead Letter Queue infrastructure (queues, exchanges, and bindings)",
                ex);
        }
    }

    /// <summary>
    /// Configuração padrão para desenvolvimento
    /// </summary>
    public static void ConfigureForDevelopment(this DeadLetterOptions options)
    {
        options.Enabled = true;
        options.MaxRetryAttempts = 3;
        options.InitialRetryDelaySeconds = 2;
        options.BackoffMultiplier = 2.0;
        options.MaxRetryDelaySeconds = 60;
        options.DeadLetterTtlHours = 24;
        options.EnableDetailedLogging = true;
        options.EnableAdminNotifications = false;
    }

    /// <summary>
    /// Configuração padrão para produção
    /// </summary>
    public static void ConfigureForProduction(this DeadLetterOptions options)
    {
        options.Enabled = true;
        options.MaxRetryAttempts = 5;
        options.InitialRetryDelaySeconds = 5;
        options.BackoffMultiplier = 2.0;
        options.MaxRetryDelaySeconds = 300;
        options.DeadLetterTtlHours = 72;
        options.EnableDetailedLogging = false;
        options.EnableAdminNotifications = true;
    }

    #endregion

    #region Message Retry Extensions

    /// <summary>
    /// Executa um handler de mensagem com retry automático e Dead Letter Queue
    /// </summary>
    public static async Task<bool> ExecuteWithRetryAsync<TMessage>(
        this TMessage message,
        Func<TMessage, CancellationToken, Task> handler,
        IServiceProvider serviceProvider,
        string sourceQueue,
        CancellationToken cancellationToken = default) where TMessage : class
    {
        var middlewareFactory = serviceProvider.GetRequiredService<IMessageRetryMiddlewareFactory>();
        var handlerType = handler.Method.DeclaringType?.FullName ?? "Unknown";

        var middleware = middlewareFactory.CreateMiddleware<TMessage>(handlerType, sourceQueue);

        return await middleware.ExecuteWithRetryAsync(message, handler, cancellationToken);
    }

    /// <summary>
    /// Configura o middleware de retry para handlers de eventos
    /// </summary>
    public static IServiceCollection AddMessageRetryMiddleware(this IServiceCollection services)
    {
        services.AddScoped<IMessageRetryMiddlewareFactory, MessageRetryMiddlewareFactory>();
        return services;
    }

    #endregion
}
