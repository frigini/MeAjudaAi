using MeAjudaAi.Shared.Messaging.Handlers;
using MeAjudaAi.Shared.Messaging.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging.DeadLetter;

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

        var options = configuration.GetSection(DeadLetterOptions.SectionName).Get<DeadLetterOptions>() ?? new DeadLetterOptions();
        configureOptions?.Invoke(options);

        if (options.Enabled)
        {
            // Registrar serviço principal baseado no ambiente (RabbitMQ por padrão)
            services.AddScoped<IDeadLetterService, RabbitMqDeadLetterService>();

            // Adicionar middleware de retry
            services.AddMessageRetryMiddleware();
        }

        return services;
    }

    /// <summary>
    /// Valida a configuração do Dead Letter Queue na aplicação
    /// </summary>
    /// <param name="host">Host da aplicação</param>
    /// <returns>Task de validação</returns>
    public static Task ValidateDeadLetterConfigurationAsync(this IHost host)
    {
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        if (!environment.IsEnvironment("Testing"))
        {
            using var scope = host.Services.CreateScope();
            IDeadLetterService? deadLetterService = null;

            try
            {
                deadLetterService = scope.ServiceProvider.GetService<IDeadLetterService>();
                
                if (deadLetterService == null)
                {
                    return Task.CompletedTask;
                }

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
                throw new InvalidOperationException(
                    $"Dead Letter Queue validation failed for {deadLetterService?.GetType().Name ?? "unknown"}", ex);
            }
        }
        
        return Task.CompletedTask;
    }

    public static Task LogDeadLetterInfrastructureInfo(this IHost host)
    {
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        
        if (!environment.IsEnvironment("Testing"))
        {
            using var scope = host.Services.CreateScope();

            try
            {
                var deadLetterService = scope.ServiceProvider.GetService<IDeadLetterService>();
                if (deadLetterService == null)
                {
                    return Task.CompletedTask;
                }

                LogRabbitMqInfrastructure<IDeadLetterService>(scope.ServiceProvider);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDeadLetterService>>();
                logger.LogError(ex, "Failed to log Dead Letter Queue infrastructure info");
                throw new InvalidOperationException(
                    "Failed to log Dead Letter Queue infrastructure info",
                    ex);
            }
        }
        
        return Task.CompletedTask;
    }

    private static void LogRabbitMqInfrastructure<TLogger>(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<TLogger>>();

        var rabbitMqOptions = services.GetService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>()?.Value;
        var dlOptions = services.GetService<Microsoft.Extensions.Options.IOptions<DeadLetterOptions>>()?.Value;
        var dlx = dlOptions?.RabbitMq.DeadLetterExchange ?? "dlx.meajudaai";
        var dlRoutingKey = dlOptions?.RabbitMq.DeadLetterRoutingKey ?? "deadletter";
        var defaultQueue = rabbitMqOptions?.DefaultQueueName ?? "meajudaai.default";

        logger.LogInformation(
            "RabbitMQ DeadLetter options loaded. Default Queue: {QueueName}. DLX Exchange: {DLX}. DL RoutingKey: {RoutingKey}. Persistence: {Persistence}. AutoDLX: {AutoDLX}. TTL: {TTLHours}h. MaxRetries: {MaxRetries}.",
            defaultQueue,
            dlx,
            dlRoutingKey,
            dlOptions?.RabbitMq.EnablePersistence ?? true,
            dlOptions?.RabbitMq.EnableAutomaticDlx ?? true,
            dlOptions?.DeadLetterTtlHours ?? 72,
            dlOptions?.MaxRetryAttempts ?? 3);
    }

    /// <summary>
    /// Configuração padrão para desenvolvimento
    /// </summary>
    /// <param name="options">Opções a serem configuradas</param>
    public static void ConfigureForDevelopment(this DeadLetterOptions options)
    {
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
        options.MaxRetryAttempts = 5;
        options.InitialRetryDelaySeconds = 5;
        options.BackoffMultiplier = 2.0;
        options.MaxRetryDelaySeconds = 300;
        options.DeadLetterTtlHours = 72;
        options.EnableDetailedLogging = false;
        options.EnableAdminNotifications = true;
    }
}
