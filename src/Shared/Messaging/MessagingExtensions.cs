using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging;

/// <summary>
/// Classe interna para categorização de logs de messaging
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2094:Classes should not be empty", 
    Justification = "Classe de categorização de logs - mantida para uso futuro")]
[ExcludeFromCodeCoverage]
internal sealed class MessagingConfiguration
{
}

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

        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();

        // Registrar implementações específicas do MessageBus condicionalmente baseado no ambiente
        // para reduzir o risco de resolução acidental em ambientes de teste
        if (environment.IsEnvironment(EnvironmentNames.Testing))
        {
            // Testing: Registra apenas NoOp - mocks serão adicionados via AddMessagingMocks()
            services.TryAddSingleton<NoOp.NoOpMessageBus>();
        }
        else
        {
            // Default: Registra RabbitMQ e NoOp (fallback)
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


    public static async Task EnsureRabbitMqInfrastructureAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var infrastructureManager = scope.ServiceProvider.GetRequiredService<IRabbitMqInfrastructureManager>();
        await infrastructureManager.EnsureInfrastructureAsync();
    }

    /// <summary>
    /// Garante a infraestrutura de messaging usando RabbitMQ
    /// </summary>
    public static async Task EnsureMessagingInfrastructureAsync(this IHost host)
    {
        await host.EnsureRabbitMqInfrastructureAsync();

        // Registrar informações sobre infraestrutura de Dead Letter Queue
        await host.LogDeadLetterInfrastructureInfo();

        // Validar configuração de Dead Letter Queue
        await host.ValidateDeadLetterConfigurationAsync();
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



    #region Message Retry Extensions

    /// <summary>
    /// Marker interface para mensagens que suportam retry automático
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2094:Classes should not be empty", 
        Justification = "Marker interface intencional para type constraints")]
    public interface IMessage
    {
        // Marker interface - não requer implementação
    }

    /// <summary>
    /// Executa um handler de mensagem com retry automático e Dead Letter Queue
    /// </summary>
    public static async Task<bool> ExecuteWithRetryAsync<TMessage>(
        this TMessage message,
        Func<TMessage, CancellationToken, Task> handler,
        IServiceProvider serviceProvider,
        string sourceQueue,
        CancellationToken cancellationToken = default) where TMessage : class, IMessage
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
        services.TryAddScoped<IMessageRetryMiddlewareFactory, MessageRetryMiddlewareFactory>();
        return services;
    }

    #endregion
}
