using MeAjudaAi.Shared.Caching.Interfaces;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Configuration;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Shared.Tests.Extensions;

/// <summary>
/// Extensões para configurar infraestrutura de testes (logging, database, messaging)
/// </summary>
public static class TestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona infraestrutura comum de testes para um módulo.
    /// Elimina boilerplate repetido em cada módulo: options, TimeProvider, logging, cache, localization, DbContext, UnitOfWork.
    /// O módulo só precisa adicionar o que é específico (mocks, queries, handlers).
    /// </summary>
    /// <typeparam name="TDbContext">Tipo do DbContext do módulo</typeparam>
    /// <param name="services">Collection de serviços</param>
    /// <param name="options">Opções de teste (opcional)</param>
    /// <param name="migrationsAssembly">Nome do assembly de migrations</param>
    /// <param name="configureDbContext">Ação extra para configurar o DbContext (opcional)</param>
    public static IServiceCollection AddCommonModuleTestInfrastructure<TDbContext>(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null,
        string? migrationsAssembly = null,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
        where TDbContext : DbContext
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);
        services.AddSingleton(TimeProvider.System);
        services.AddTestLogging();
        services.AddTestCache(options.Cache);
        services.AddLocalization();
        services.AddSingleton<ICacheService, TestCacheService>();

        // DbContext com padrão compartilhado
        services.AddDbContext<TDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                if (migrationsAssembly != null)
                    npgsqlOptions.MigrationsAssembly(migrationsAssembly);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            });

            dbOptions.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            // Configuração adicional específica do módulo
            configureDbContext?.Invoke(dbOptions);
        });

        return services;
    }

    /// <summary>
    /// Adiciona configuração básica de logging para testes
    /// </summary>
    public static IServiceCollection AddTestLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            var silentMode = Environment.GetEnvironmentVariable("TEST_SILENT_LOGGING");
            if (!string.IsNullOrEmpty(silentMode) && silentMode.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                builder.ConfigureSilentLogging();
            }
            else
            {
                builder.ConfigureTestLogging();
            }
        });

        return services;
    }

    /// <summary>
    /// Adiciona configuração básica de cache para testes
    /// </summary>
    public static IServiceCollection AddTestCache(this IServiceCollection services, TestCacheOptions? options = null)
    {
        options ??= new TestCacheOptions();

        if (options.Enabled)
        {
            // Para testes simples, usar cache em memória ao invés de Redis
            services.AddMemoryCache();
        }

        return services;
    }

    /// <summary>
    /// Adiciona mock genérico do message bus para testes
    /// </summary>
    public static IServiceCollection AddTestMessageBus(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Scoped<IMessageBus, MockMessageBus>());
        return services;
    }

    /// <summary>
    /// Configura um DbContext genérico para usar com TestContainers PostgreSQL
    /// </summary>
    public static IServiceCollection AddTestDatabase<TDbContext>(
        this IServiceCollection services,
        TestDatabaseOptions options,
        string migrationsAssembly)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();

            string connectionString;
            try
            {
                connectionString = container.GetConnectionString();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not mapped"))
            {
                // Aguarda um pouco e tenta novamente - o container pode ainda estar iniciando
                Thread.Sleep(2000);
                try
                {
                    connectionString = container.GetConnectionString();
                }
                catch
                {
                    throw new InvalidOperationException(
                        "PostgreSQL container is not running or ports are not mapped. " +
                        "Container may still be starting up. Please ensure SharedTestContainers.StartAllAsync() " +
                        "was called and container is fully ready before creating DbContext.", ex);
                }
            }

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(migrationsAssembly);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Schema);
                npgsqlOptions.CommandTimeout(60);
            });
        });

        return services;
    }
}

/// <summary>
/// Mock genérico do message bus para testes
/// </summary>
internal class MockMessageBus : IMessageBus
{
    private readonly List<object> _recordedMessages = new();

    public IReadOnlyList<object> RecordedMessages => _recordedMessages.AsReadOnly();

    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        _recordedMessages.Add(message);
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        _recordedMessages.Add(@event);
        return Task.CompletedTask;
    }

    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task>? handler = null, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void ClearMessages()
    {
        _recordedMessages.Clear();
    }
}
