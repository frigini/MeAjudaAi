using DotNet.Testcontainers.Builders;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Fixture compartilhada para testes E2E usando TestContainers.
/// Implementa IClassFixture para compartilhar containers entre testes da mesma classe.
/// Reduz overhead de criação de containers de ~6s por teste para ~6s por classe.
/// </summary>
public class TestContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;
    private RedisContainer _redisContainer = null!;
    private AzuriteContainer _azuriteContainer = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;
    public string RedisConnectionString { get; private set; } = null!;
    public string AzuriteConnectionString { get; private set; } = null!;

    public static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public async ValueTask InitializeAsync()
    {
        // Retry logic com exponential backoff para lidar com transient Docker failures
        const int maxRetries = 3;
        const int baseDelayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await InitializeContainersAsync();
                await InitializeFactoryAsync();
                return; // Success
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt - 1));
                Console.WriteLine($"⚠️ Attempt {attempt}/{maxRetries} failed: {ex.Message}. Retrying in {delay.TotalSeconds}s...");
                await Task.Delay(delay);
            }
        }

        // Se chegou aqui, todas as tentativas falharam
        throw new InvalidOperationException($"Failed to initialize TestContainers after {maxRetries} attempts. Ensure Docker Desktop is running and healthy.");
    }

    private async Task InitializeContainersAsync()
    {
        // Configurar containers com timeouts aumentados (1min → 5min)
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:16-3.4")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6379))
            .Build();

        _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithCleanUp(true)
            .Build();

        // Iniciar containers em paralelo para economizar tempo
        var startTasks = new[]
        {
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _azuriteContainer.StartAsync()
        };

        await Task.WhenAll(startTasks);

        // Armazenar connection strings
        PostgresConnectionString = _postgresContainer.GetConnectionString();
        RedisConnectionString = _redisContainer.GetConnectionString();
        AzuriteConnectionString = _azuriteContainer.GetConnectionString();

        Console.WriteLine("✅ TestContainers initialized successfully");
        Console.WriteLine($"📦 PostgreSQL: Host={_postgresContainer.Hostname}:{_postgresContainer.GetMappedPublicPort(5432)}, Database=meajudaai_test");
        Console.WriteLine($"📦 Redis: Host={_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}");
        Console.WriteLine($"📦 Azurite: Host={_azuriteContainer.Hostname}:{_azuriteContainer.GetMappedPublicPort(10000)}");
    }

    private async Task InitializeFactoryAsync()
    {
#pragma warning disable CA2000 // Dispose é gerenciado por IAsyncLifetime.DisposeAsync
        _factory = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        // All modules share the same test database instance
                        ["ConnectionStrings:DefaultConnection"] = PostgresConnectionString,
                        ["ConnectionStrings:meajudaai-db"] = PostgresConnectionString,
                        ["ConnectionStrings:UsersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:ProvidersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:DocumentsDb"] = PostgresConnectionString,
                        ["ConnectionStrings:Redis"] = RedisConnectionString,
                        ["Azure:Storage:ConnectionString"] = AzuriteConnectionString,
                        ["Hangfire:Enabled"] = "false",
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft"] = "Error",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Keycloak:Enabled"] = "false",
                        ["Cache:Enabled"] = "false",
                        ["Cache:ConnectionString"] = RedisConnectionString,
                        ["AdvancedRateLimit:General:Enabled"] = "false",
                        ["AdvancedRateLimit:Anonymous:RequestsPerMinute"] = "10000",
                        ["AdvancedRateLimit:Anonymous:RequestsPerHour"] = "100000",
                        ["AdvancedRateLimit:Anonymous:RequestsPerDay"] = "1000000",
                        ["AdvancedRateLimit:Authenticated:RequestsPerMinute"] = "10000",
                        ["AdvancedRateLimit:Authenticated:RequestsPerHour"] = "100000",
                        ["AdvancedRateLimit:Authenticated:RequestsPerDay"] = "1000000",
                        ["AdvancedRateLimit:General:WindowInSeconds"] = "60",
                        ["AdvancedRateLimit:General:EnableIpWhitelist"] = "true",
                        ["RateLimit:DefaultRequestsPerMinute"] = "999999",
                        ["RateLimit:AuthRequestsPerMinute"] = "999999",
                        ["RateLimit:SearchRequestsPerMinute"] = "999999",
                        ["RateLimit:WindowInSeconds"] = "3600"
                    });

                    config.AddEnvironmentVariables("MEAJUDAAI_TEST_");
                });

                builder.ConfigureServices(services =>
                {
                    // Configurar logging mínimo para testes
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Error);
                    });

                    // Mock de serviços externos
                    ConfigureMockServices(services);

                    // Reconfigurar DbContexts
                    ReconfigureDbContexts(services);
                });
            });

        ApiClient = _factory.CreateClient();
        Services = _factory.Services;

        // Aplicar migrations e seed inicial
        await ApplyMigrationsAsync();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        // Mock Keycloak
        var keycloakDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IKeycloakService));
        if (keycloakDescriptor != null)
            services.Remove(keycloakDescriptor);
        services.AddSingleton<IKeycloakService, MockKeycloakService>();

        // Mock Blob Storage
        var blobDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlobStorageService));
        if (blobDescriptor != null)
            services.Remove(blobDescriptor);
        services.AddSingleton<IBlobStorageService, MockBlobStorageService>();
    }

    private void ReconfigureDbContexts(IServiceCollection services)
    {
        ReconfigureDbContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);

        // PostgresOptions para SearchProviders (Dapper)
        var postgresOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PostgresOptions));
        if (postgresOptionsDescriptor != null)
            services.Remove(postgresOptionsDescriptor);

        services.AddSingleton(new PostgresOptions
        {
            ConnectionString = PostgresConnectionString
        });

        // DatabaseMetrics
        if (!services.Any(d => d.ServiceType == typeof(DatabaseMetrics)))
        {
            services.AddSingleton<DatabaseMetrics>();
        }

        // DapperConnection
        var dapperDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDapperConnection));
        if (dapperDescriptor != null)
            services.Remove(dapperDescriptor);

        services.AddScoped<IDapperConnection, DapperConnection>();
    }

    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(PostgresConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
                npgsqlOptions.UseNetTopologySuite();
                npgsqlOptions.CommandTimeout(60); // 1 minuto timeout para queries
            });
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });
    }

    private async Task ApplyMigrationsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            // Aplicar migrations para cada módulo
            await ApplyMigrationForContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);

            Console.WriteLine("✅ Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
            throw;
        }
    }

    private static async Task ApplyMigrationForContext<TContext>(IServiceProvider services) where TContext : DbContext
    {
        var context = services.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Limpa dados do banco entre testes para garantir isolamento.
    /// Mantém schema/migrations, apenas remove dados.
    /// </summary>
    public async Task CleanupDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;

        // Limpar cada DbContext
        await CleanupContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);
    }

    private static async Task CleanupContext<TContext>(IServiceProvider services) where TContext : DbContext
    {
        var context = services.GetRequiredService<TContext>();

        // Delete all data but keep schema
        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            if (!string.IsNullOrEmpty(tableName))
            {
                // Build schema-qualified table name
                var qualifiedTableName = string.IsNullOrEmpty(schema) || schema == "public"
                    ? $"\"{tableName}\""
                    : $"\"{schema}\".\"{tableName}\"";

#pragma warning disable EF1002 // Risk of SQL injection - table/schema names come from EF metadata, not user input
                await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {qualifiedTableName} CASCADE");
#pragma warning restore EF1002
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        ApiClient?.Dispose();

        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }

        // Parar containers em paralelo
        var stopTasks = new List<Task>();

        if (_postgresContainer != null)
            stopTasks.Add(_postgresContainer.DisposeAsync().AsTask());

        if (_redisContainer != null)
            stopTasks.Add(_redisContainer.DisposeAsync().AsTask());

        if (_azuriteContainer != null)
            stopTasks.Add(_azuriteContainer.DisposeAsync().AsTask());

        await Task.WhenAll(stopTasks);

        Console.WriteLine("✅ TestContainers disposed successfully");
    }
}
