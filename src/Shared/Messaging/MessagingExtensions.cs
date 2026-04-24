using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Factories;
using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Messaging.NoOp;
using MeAjudaAi.Shared.Messaging.Options;
using MeAjudaAi.Shared.Messaging.RabbitMq;
using MeAjudaAi.Shared.Messaging.Rebus;
using MeAjudaAi.Shared.Messaging.Rebus.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Rebus.Topic;

namespace MeAjudaAi.Shared.Messaging;

/// <summary>
/// Classe interna para categorização de logs de messaging
/// </summary>
internal sealed class MessagingConfiguration
{
}

/// <summary>
/// Extension methods consolidados para configuração de Messaging, Dead Letter Queue e Message Retry
/// </summary>
public static class MessagingExtensions
{
    private const string UseNewtonsoftJsonKey = "Messaging:UseNewtonsoftJson";

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
        
        // Fallback para ConnectionString do Aspire se não fornecida explicitamente
        services.PostConfigure<RabbitMqOptions>(options => 
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                var aspireConn = configuration.GetConnectionString("rabbitmq");
                if (!string.IsNullOrWhiteSpace(aspireConn))
                {
                    options.ConnectionString = aspireConn;
                }
            }
        });

        services.AddSingleton(provider => provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value);

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
                
                // Fail-fast validation
                if (string.IsNullOrWhiteSpace(options.ConnectionString) && 
                    (string.IsNullOrWhiteSpace(options.Host) || options.Host == "localhost"))
                {
                    // Se não tem connection string e o host é o default (ou vazio), 
                    // consideramos que a configuração está ausente ou incompleta.
                    throw new InvalidOperationException("RabbitMQ configuration is missing or incomplete. Please provide a ConnectionString or Host/Username/Password.");
                }

                var connectionString = options.BuildConnectionString();
                
                configure
                    .Transport(t => t.UseRabbitMq(connectionString, options.DefaultQueueName));

                var useNewtonsoftJson = configuration.GetValue<bool>(UseNewtonsoftJsonKey, false);
                if (useNewtonsoftJson)
                {
                    configure.Serialization(s => s.UseNewtonsoftJson());
                }

                return configure
                    .Options(o => 
                    {
                        o.SetMaxParallelism(20);
                        o.SetNumberOfWorkers(2);
                        o.Decorate<ITopicNameConvention>(c => new AttributeTopicNameConvention(c.Get<ITopicNameConvention>()));
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


    public static async Task EnsureMessagingInfrastructureAsync(this IHost host)
    {
        var isEnabled = host.Services.GetRequiredService<IConfiguration>().GetValue<bool>("Messaging:Enabled", true);
        if (!isEnabled)
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IRabbitMqInfrastructureManager>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MessagingConfiguration>>();

        var useNewtonsoftJson = scope.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<bool>(UseNewtonsoftJsonKey, false);
        if (useNewtonsoftJson)
        {
            logger.LogInformation("Messaging: Newtonsoft.Json is ENABLED. Using legacy serializer.");
        }

        try
        {
            logger.LogInformation("Ensuring messaging infrastructure (Queues/Exchanges)...");
            await manager.EnsureInfrastructureAsync();
            logger.LogInformation("Messaging infrastructure verified.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure messaging infrastructure.");
            throw;
        }
    }

    #region Message Retry

    /// <summary>
    /// Adiciona o middleware de retry para mensagens
    /// </summary>
    public static IServiceCollection AddMessageRetryMiddleware(this IServiceCollection services)
    {
        services.TryAddScoped<IMessageRetryMiddlewareFactory, MessageRetryMiddlewareFactory>();
        return services;
    }

    #endregion
}
