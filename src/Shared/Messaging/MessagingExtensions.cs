using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.Rebus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.Routing.TypeBased;

namespace MeAjudaAi.Shared.Messaging;

/// <summary>
/// Classe interna para categorização de logs de messaging
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class MessagingConfiguration
{
}

/// <summary>
/// Extension methods consolidados para configuração de Messaging, Dead Letter Queue e Message Retry
/// </summary>
[ExcludeFromCodeCoverage]
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

        // Registro das configurações do RabbitMQ via Options pipeline
        services.Configure<RabbitMqOptions>(configuration.GetSection("Messaging:RabbitMQ"));
        services.AddSingleton(provider => provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value);

        // Registro direto das configurações do MessageBus
        services.AddSingleton(provider =>
        {
            var options = new MessageBusOptions();
            configureOptions?.Invoke(options);
            return options;
        });

        services.AddSingleton<IEventTypeRegistry, EventTypeRegistry>();

        // Registrar implementações específicas do MessageBus condicionalmente baseado no ambiente
        if (environment.IsEnvironment(EnvironmentNames.Testing))
        {
            services.TryAddSingleton<NoOp.NoOpMessageBus>();
        }
        else
        {
            // Configuração do Rebus
            services.AddRebus((configure, provider) => {
                var options = provider.GetRequiredService<RabbitMqOptions>();
                return configure
                    .Transport(t => t.UseRabbitMq(options.ConnectionString, options.DefaultQueueName))
                    .Options(o => 
                    {
                        o.SetMaxParallelism(20);
                        o.SetNumberOfWorkers(2);
                    })
                    .Routing(r => r.TypeBased());
            });

            services.TryAddSingleton<RebusMessageBus>();
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
