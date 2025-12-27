using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Configuration;

/// <summary>
/// Configurações de logging otimizadas para testes de todos os módulos.
/// Reduz verbosidade e filtra logs desnecessários durante execução de testes.
/// </summary>
public static class TestLoggingConfiguration
{
    /// <summary>
    /// Configura logging mínimo para testes com filtros para reduzir ruído
    /// </summary>
    public static ILoggingBuilder ConfigureTestLogging(this ILoggingBuilder builder)
    {
        builder.ClearProviders();

        // Adiciona console provider apenas se não estiver em modo de teste silencioso
        var verboseMode = Environment.GetEnvironmentVariable("TEST_VERBOSE_LOGGING");
        if (!string.IsNullOrEmpty(verboseMode) && verboseMode.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddConsole();
        }

        // Filtros para reduzir logs desnecessários
        builder.AddFilter("System", LogLevel.Warning);
        builder.AddFilter("Microsoft", LogLevel.Warning);
        builder.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
        builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);

        // Filtros específicos para TestContainers
        builder.AddFilter("Testcontainers", LogLevel.Warning);
        builder.AddFilter("Docker.DotNet", LogLevel.Error);

        // Filtros para Aspire e componentes relacionados
        builder.AddFilter("Aspire", LogLevel.Warning);
        builder.AddFilter("Microsoft.Extensions.ServiceDiscovery", LogLevel.Warning);
        builder.AddFilter("Microsoft.Extensions.Http.Resilience", LogLevel.Warning);

        // Filtros para RabbitMQ e messaging
        builder.AddFilter("RabbitMQ", LogLevel.Warning);
        builder.AddFilter("MassTransit", LogLevel.Warning);
        builder.AddFilter("EasyNetQ", LogLevel.Warning);

        // Filtros para Redis
        builder.AddFilter("StackExchange.Redis", LogLevel.Warning);

        // Filtros para PostgreSQL/Npgsql
        builder.AddFilter("Npgsql", LogLevel.Warning);

        // Mantém logs da aplicação em nível Info para debugging de testes
        builder.AddFilter("MeAjudaAi", LogLevel.Information);

        // Level mínimo global
        builder.SetMinimumLevel(LogLevel.Warning);

        return builder;
    }

    /// <summary>
    /// Configura logging completamente silencioso para testes de performance
    /// </summary>
    public static ILoggingBuilder ConfigureSilentLogging(this ILoggingBuilder builder)
    {
        builder.ClearProviders();
        builder.SetMinimumLevel(LogLevel.Critical);

        // Adiciona provider no-op para evitar warnings
        builder.Services.AddSingleton<ILoggerProvider, NoOpLoggerProvider>();

        return builder;
    }
}

/// <summary>
/// Logger provider que não faz nada - para testes completamente silenciosos
/// </summary>
internal class NoOpLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => NoOpLogger.Instance;
    public void Dispose() { }
}

/// <summary>
/// Logger que não faz nada - para testes completamente silenciosos
/// </summary>
internal class NoOpLogger : ILogger
{
    public static readonly NoOpLogger Instance = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

internal class NullScope : IDisposable
{
    public static readonly NullScope Instance = new();
    public void Dispose() { }
}
