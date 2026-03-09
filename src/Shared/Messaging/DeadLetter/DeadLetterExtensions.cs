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
    /// <summary>
    /// Configures dead-letter processing: binds DeadLetterOptions from configuration, registers dead-letter service implementations, and adds message retry middleware.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configuration">Configuration containing the DeadLetterOptions section.</param>
    /// <param name="configureOptions">Optional callback to further configure DeadLetterOptions.</param>
    /// <returns>The updated IServiceCollection for chaining.</returns>
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
        services.AddScoped<NoOpDeadLetterService>();

        // Registrar serviço principal baseado no ambiente (RabbitMQ por padrão)
        services.AddScoped<IDeadLetterService, RabbitMqDeadLetterService>();

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
    /// <summary>
    /// Registers RabbitMQ-backed dead letter services and related retry middleware using settings from configuration.
    /// </summary>
    /// <param name="configuration">Configuration root used to bind <see cref="DeadLetterOptions"/> from its configuration section.</param>
    /// <param name="configureOptions">Optional callback to further configure <see cref="DeadLetterOptions"/> after binding.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
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
            throw new InvalidOperationException(
                $"Dead Letter Queue validation failed for {deadLetterService?.GetType().Name ?? "unknown"}", ex);
        }
    }

    /// <summary>
    /// Garante que a infraestrutura de Dead Letter Queue está criada
    /// </summary>
    /// <param name="host">Host da aplicação</param>
    /// <summary>
    /// Ensures the dead-letter infrastructure for RabbitMQ is present or will be created dynamically in supported environments.
    /// </summary>
    /// <param name="host">The application host used to create a service scope to resolve environment and logging services.</param>
    /// <returns>A task that completes when the infrastructure check and any dynamic creation decision have finished.</returns>
    /// <exception cref="InvalidOperationException">Thrown if ensuring the dead-letter infrastructure fails; the original exception is preserved as the inner exception.</exception>
    public static Task EnsureDeadLetterInfrastructureAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        try
        {
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHostEnvironment>>();

            if (environment.IsDevelopment() || environment.IsProduction())
            {
                // Para RabbitMQ, a infraestrutura é criada dinamicamente quando necessário
                logger.LogInformation("Dead Letter infrastructure for RabbitMQ will be created dynamically");
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
    /// Registra informações sobre a infraestrutura de Dead Letter Queue
    /// <summary>
    /// Logs that RabbitMQ dead-letter infrastructure will be created dynamically when the host is running in Development or Production.
    /// </summary>
    /// <returns>A completed Task.</returns>
    /// <exception cref="InvalidOperationException">Thrown if determining or logging the dead-letter infrastructure information fails; the original exception is preserved as the inner exception.</exception>
    public static Task LogDeadLetterInfrastructureInfo(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        try
        {
            var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDeadLetterService>>();

            if (environment.IsDevelopment() || environment.IsProduction())
            {
                // Para RabbitMQ, a infraestrutura é criada dinamicamente quando necessário
                logger.LogInformation("Dead Letter infrastructure for RabbitMQ will be created dynamically");
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDeadLetterService>>();
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
