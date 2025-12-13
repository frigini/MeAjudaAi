using MeAjudaAi.Shared.Messaging.DeadLetter;
using MeAjudaAi.Shared.Messaging.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.Extensions;

/// <summary>
/// Extensões para configurar o sistema de Dead Letter Queue
/// </summary>
public static class DeadLetterExtensions
{
    /// <summary>
    /// Adiciona o sistema de Dead Letter Queue ao container de dependências
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="configureOptions">Configuração adicional das opções</param>
    /// <returns>Service collection para chaining</returns>
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
        services.AddScoped<IDeadLetterServiceFactory, EnvironmentBasedDeadLetterServiceFactory>();

        // Registrar serviço principal baseado no ambiente
        services.AddScoped<IDeadLetterService>(serviceProvider =>
        {
            var factory = serviceProvider.GetRequiredService<IDeadLetterServiceFactory>();
            return factory.CreateDeadLetterService();
        });

        // Adicionar middleware de retry
        services.AddMessageRetryMiddleware();

        return services;
    }

    /// <summary>
    /// Configura dead letter queue específico para RabbitMQ
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="configureOptions">Configuração adicional das opções</param>
    /// <returns>Service collection para chaining</returns>
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
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="configureOptions">Configuração adicional das opções</param>
    /// <returns>Service collection para chaining</returns>
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
    /// <param name="host">Host da aplicação</param>
    /// <returns>Task de validação</returns>
    public static Task ValidateDeadLetterConfigurationAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        IDeadLetterService? deadLetterService = null;

        try
        {
            deadLetterService = scope.ServiceProvider.GetRequiredService<IDeadLetterService>();
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<IDeadLetterService>>();

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
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<IDeadLetterService>>();
            logger.LogError(ex, "Failed to validate Dead Letter Queue configuration. Service: {ServiceType}",
                deadLetterService?.GetType().Name ?? "unknown");
            throw;
        }
    }

    /// <summary>
    /// Garante que a infraestrutura de Dead Letter Queue está criada
    /// </summary>
    /// <param name="host">Host da aplicação</param>
    /// <returns>Task de criação da infraestrutura</returns>
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
    /// <param name="options">Opções a serem configuradas</param>
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
    /// <param name="options">Opções a serem configuradas</param>
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
}
