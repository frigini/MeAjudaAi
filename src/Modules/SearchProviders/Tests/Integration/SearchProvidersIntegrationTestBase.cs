using System.Data.Common;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Integration;

/// <summary>
/// Classe base para testes de integração do módulo SearchProviders.
/// Usa Testcontainers PostgreSQL com extensão PostGIS.
/// </summary>
public abstract class SearchProvidersIntegrationTestBase : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ServiceProvider? _serviceProvider;
    private readonly string _testClassId;

    protected SearchProvidersIntegrationTestBase()
    {
        _testClassId = $"{GetType().Name}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Inicialização executada antes de cada classe de teste
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Criar container PostgreSQL com PostGIS
        _container = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:16-3.4") // Imagem com PostGIS
            .WithDatabase($"search_test_{_testClassId}")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithPortBinding(0, true) // Porta aleatória
            .Build();

        await _container.StartAsync();

        // Configurar serviços
        var services = new ServiceCollection();

        services.AddSingleton(_container);

        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);
            builder.AddConsole();
        });

        // Configurar DbContext com connection string do container
        services.AddDbContext<SearchProvidersDbContext>(options =>
        {
            options.UseNpgsql(
                _container.GetConnectionString(),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search");
                });

            // Use same naming convention as production
            options.UseSnakeCaseNamingConvention();
        });

        // Registrar PostgresOptions para Dapper
        var connectionString = _container.GetConnectionString();
        services.AddSingleton(new PostgresOptions { ConnectionString = connectionString });

        // Registrar métricas (IMeterFactory requerido por DatabaseMetrics)
        services.AddMetrics();

        // Registrar DatabaseMetrics + Interceptor (via AddDatabaseMonitoring do Shared)
        services.AddDatabaseMonitoring();

        // Registrar Dapper connection (necessário para SearchableProviderRepository)
        services.AddScoped<IDapperConnection, DapperConnection>();

        // Registrar repositório
        services.AddScoped<MeAjudaAi.Modules.SearchProviders.Domain.Repositories.ISearchableProviderRepository,
            MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Repositories.SearchableProviderRepository>();

        _serviceProvider = services.BuildServiceProvider();

        // Inicializar banco de dados
        await InitializeDatabaseAsync();
    }

    /// <summary>
    /// Inicializa o banco de dados com PostGIS
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        var dbContext = _serviceProvider!.GetRequiredService<SearchProvidersDbContext>();

        // Criar banco de dados
        await dbContext.Database.EnsureCreatedAsync();

        // Verificar se PostGIS está disponível
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            var wasOpen = connection.State == System.Data.ConnectionState.Open;

            if (!wasOpen)
            {
                await connection.OpenAsync();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT PostGIS_Version()";
                var version = await command.ExecuteScalarAsync();
                if (version == null)
                {
                    throw new InvalidOperationException("PostGIS extension is not available in the test database");
                }
            }
            finally
            {
                if (!wasOpen && connection.State == System.Data.ConnectionState.Open)
                {
                    await connection.CloseAsync();
                }
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("PostGIS extension is not available in the test database", ex);
        }

        // Verificar isolamento
        var count = await dbContext.SearchableProviders.CountAsync();
        if (count > 0)
        {
            throw new InvalidOperationException($"Database isolation failed: found {count} existing providers");
        }
    }

    /// <summary>
    /// Cria um SearchableProvider para teste
    /// </summary>
    protected SearchableProvider CreateTestSearchableProvider(
        string name,
        double latitude,
        double longitude,
        ESubscriptionTier tier = ESubscriptionTier.Free,
        string? description = null,
        string? city = null,
        string? state = null)
    {
        var providerId = Guid.NewGuid();
        var location = new GeoPoint(latitude, longitude);

        var provider = SearchableProvider.Create(
            providerId,
            name,
            location,
            tier,
            description,
            city,
            state);

        return provider;
    }

    /// <summary>
    /// Cria um SearchableProvider com ProviderId específico para teste
    /// </summary>
    protected SearchableProvider CreateTestSearchableProviderWithProviderId(
        Guid providerId,
        string name,
        double latitude,
        double longitude,
        ESubscriptionTier tier = ESubscriptionTier.Free,
        string? description = null,
        string? city = null,
        string? state = null)
    {
        var location = new GeoPoint(latitude, longitude);

        var provider = SearchableProvider.Create(
            providerId,
            name,
            location,
            tier,
            description,
            city,
            state);

        return provider;
    }

    /// <summary>
    /// Persiste um SearchableProvider no banco de dados
    /// </summary>
    protected async Task<SearchableProvider> PersistSearchableProviderAsync(SearchableProvider provider)
    {
        var dbContext = GetService<SearchProvidersDbContext>();
        await dbContext.SearchableProviders.AddAsync(provider);
        await dbContext.SaveChangesAsync();
        return provider;
    }

    /// <summary>
    /// Limpa dados das tabelas
    /// </summary>
    protected async Task CleanupDatabase()
    {
        var dbContext = GetService<SearchProvidersDbContext>();

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE search.searchable_providers CASCADE;");
        }
        catch
        {
            await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM search.searchable_providers;");
        }

        var remainingCount = await dbContext.SearchableProviders.CountAsync();
        if (remainingCount > 0)
        {
            throw new InvalidOperationException($"Database cleanup failed: {remainingCount} providers remain");
        }
    }

    /// <summary>
    /// Limpeza executada após cada classe de teste
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider != null)
        {
            await _serviceProvider.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Obtém um serviço do provider
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized");
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Cria um escopo de serviços
    /// </summary>
    protected IServiceScope CreateScope()
    {
        if (_serviceProvider == null)
            throw new InvalidOperationException("Service provider not initialized");
        return _serviceProvider.CreateScope();
    }
}
