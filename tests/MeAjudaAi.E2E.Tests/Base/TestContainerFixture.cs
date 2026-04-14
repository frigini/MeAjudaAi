using DotNet.Testcontainers.Builders;
using Bogus;
using MeAjudaAi.ApiService;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.E2E.Tests.Base.Helpers;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Json;
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
    private static PostgreSqlContainer? _postgresContainer;
    private static RedisContainer? _redisContainer;
    private static AzuriteContainer? _azuriteContainer;
    private static readonly SemaphoreSlim _initializationLock = new(1, 1);
    private static bool _containersInitialized = false;
    private static bool _migrationsApplied = false;

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;
    public string RedisConnectionString { get; private set; } = null!;
    public string AzuriteConnectionString { get; private set; } = null!;
    public Faker Faker { get; } = new();

    /// <summary>
    /// Define se o sistema de mensagens síncrono e processamento de eventos devem estar ativos.
    /// Por padrão é falso para isolar testes unitários/E2E simples.
    /// </summary>
    public virtual bool EnableEventsAndMessageBus => false;

    public static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public async ValueTask InitializeAsync()
    {
        // One-time initialization for the entire test run
        if (!_containersInitialized)
        {
            await _initializationLock.WaitAsync();
            try
            {
                if (!_containersInitialized)
                {
                    await InitializeContainersAsync();
                    _containersInitialized = true;
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }
        
        // Populate properties for the current instance (legacy support)
        if (_postgresContainer != null) PostgresConnectionString = _postgresContainer.GetConnectionString();
        if (_redisContainer != null) RedisConnectionString = _redisContainer.GetConnectionString();
        if (_azuriteContainer != null) AzuriteConnectionString = _azuriteContainer.GetConnectionString();

        // Initialize WebApplicationFactory for THIS test class instance
        await InitializeFactoryAsync();

        // One-time migration application for the entire test run
        if (!_migrationsApplied)
        {
            await _initializationLock.WaitAsync();
            try
            {
                if (!_migrationsApplied)
                {
                    await ApplyMigrationsAsync();
                    _migrationsApplied = true;
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }
    }

    private async Task InitializeContainersAsync()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Configurar containers com timeouts aumentados para WSL2/Docker Desktop (Windows)
        if (_postgresContainer == null)
        {
            _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
                .WithDatabase("meajudaai_test")
                .WithUsername("postgres")
                .WithPassword("test123")
                .WithCleanUp(true)
                .Build();
        }

        if (_redisContainer == null)
        {
            _redisContainer = new RedisBuilder("redis:7-alpine")
                .WithCleanUp(true)
                .Build();
        }

        if (_azuriteContainer == null)
        {
            _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
                .WithCleanUp(true)
                .Build();
        }

        // Iniciar containers em paralelo
        var startTasks = new List<Task>();
        startTasks.Add(_postgresContainer.StartAsync());
        startTasks.Add(_redisContainer.StartAsync());
        startTasks.Add(_azuriteContainer.StartAsync());

        await Task.WhenAll(startTasks);

        // Armazenar connection strings
        PostgresConnectionString = _postgresContainer.GetConnectionString();
        RedisConnectionString = _redisContainer.GetConnectionString();
        AzuriteConnectionString = _azuriteContainer.GetConnectionString();

        Console.WriteLine("✅ TestContainers initialized successfully");
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
                        ["ConnectionStrings:DefaultConnection"] = PostgresConnectionString,
                        ["ConnectionStrings:meajudaai-db"] = PostgresConnectionString,
                        ["ConnectionStrings:Users"] = PostgresConnectionString,
                        ["ConnectionStrings:UsersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:ProvidersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:DocumentsDb"] = PostgresConnectionString,
                        ["ConnectionStrings:SearchProvidersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:Redis"] = RedisConnectionString,
                        ["Migrations:Enabled"] = "false",
                        ["Azure:Storage:ConnectionString"] = AzuriteConnectionString,
                        ["Hangfire:Enabled"] = "false",
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft"] = "Error",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Keycloak:Enabled"] = "false",
                        ["Keycloak:ClientSecret"] = "test-secret",
                        ["Keycloak:AdminUsername"] = "test-admin",
                        ["Keycloak:AdminPassword"] = "test-password",
                        ["Cache:Enabled"] = "false", // Sincronizado com BaseTestContainerTest
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
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Error);
                    });

                    ConfigureMockServices(services);
                    ReconfigureDbContexts(services);
                });
            });

        var contextPropagationHandler = new TestContextAwareHandler
        {
            InnerHandler = _factory.Server.CreateHandler()
        };
        
        ApiClient = new HttpClient(contextPropagationHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        
        Services = _factory.Services;
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        services.AddAuthentication(MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.SchemeName)
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler>(
                MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.SchemeName,
                _ => { });
        
        if (!services.Any(d => d.ServiceType == typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService)))
        {
            services.AddAuthorization();
        }
        
        var keycloakDescriptors = services.Where(d => d.ServiceType == typeof(IKeycloakService)).ToList();
        foreach (var d in keycloakDescriptors) services.Remove(d);
        services.AddSingleton<IKeycloakService, MockKeycloakService>();

        var blobDescriptors = services.Where(d => d.ServiceType == typeof(IBlobStorageService)).ToList();
        foreach (var d in blobDescriptors) services.Remove(d);
        services.AddSingleton<IBlobStorageService, MockBlobStorageService>();

        var ocrDescriptors = services.Where(d => d.ServiceType == typeof(IDocumentIntelligenceService)).ToList();
        foreach (var d in ocrDescriptors) services.Remove(d);
        services.AddSingleton<IDocumentIntelligenceService, MockDocumentIntelligenceService>();

        // Message Bus Condicional para E2E
        var busDescriptors = services.Where(d => d.ServiceType == typeof(MeAjudaAi.Shared.Messaging.IMessageBus)).ToList();
        foreach (var d in busDescriptors) services.Remove(d);

        var domainProcessorDescriptors = services.Where(d => d.ServiceType == typeof(MeAjudaAi.Shared.Events.IDomainEventProcessor)).ToList();
        foreach (var d in domainProcessorDescriptors) services.Remove(d);

        if (EnableEventsAndMessageBus)
        {
            services.AddSingleton<MeAjudaAi.Shared.Messaging.IMessageBus, MeAjudaAi.E2E.Tests.Infrastructure.SynchronousInMemoryMessageBus>();
            services.AddScoped<MeAjudaAi.Shared.Events.IDomainEventProcessor, MeAjudaAi.Shared.Events.DomainEventProcessor>();
        }
        else
        {
            services.AddSingleton<MeAjudaAi.Shared.Messaging.IMessageBus, MockMessageBus>();
            services.AddScoped<MeAjudaAi.Shared.Events.IDomainEventProcessor, MockDomainEventProcessor>();
        }
    }

    private void ReconfigureDbContexts(IServiceCollection services)
    {
        ReconfigureDbContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>(services);

        var postgresOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PostgresOptions));
        if (postgresOptionsDescriptor != null)
            services.Remove(postgresOptionsDescriptor);

        services.AddSingleton(new PostgresOptions { ConnectionString = PostgresConnectionString });

        if (!services.Any(d => d.ServiceType == typeof(DatabaseMetrics)))
        {
            services.AddSingleton<DatabaseMetrics>();
        }

        var dapperDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDapperConnection));
        if (dapperDescriptor != null)
            services.Remove(dapperDescriptor);

        services.AddScoped<IDapperConnection, DapperConnection>();
    }

    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        // Console.WriteLine($"[DEBUG] Reconfiguring DbContext: {contextName}");
        
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(PostgresConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextSchemaHelper.GetSchemaName(contextName));
                npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
                npgsqlOptions.UseNetTopologySuite();
                npgsqlOptions.CommandTimeout(120);
            });
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
    }

    private async Task ApplyMigrationsAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>());

            Console.WriteLine("✅ Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error applying migrations: {ex.Message}");
            throw;
        }
    }

    public async Task CleanupDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var services = scope.ServiceProvider;

        await CleanupContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>(services);
        await CleanupContext<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>(services);

        if (_redisContainer != null)
        {
            await _redisContainer.ExecAsync(new[] { "redis-cli", "FLUSHALL" });
        }
    }

    private static async Task CleanupContext<TContext>(IServiceProvider services) where TContext : DbContext
    {
        var context = services.GetRequiredService<TContext>();
        var tableNames = new List<string>();

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema();

            if (!string.IsNullOrEmpty(tableName))
            {
                var qualifiedTableName = string.IsNullOrEmpty(schema) || schema == "public"
                    ? $"\"{tableName}\""
                    : $"\"{schema}\".\"{tableName}\"";
                
                tableNames.Add(qualifiedTableName);
            }
        }

        if (tableNames.Count > 0)
        {
            var uniqueTables = tableNames.Distinct().ToList();
            var batchSql = $"TRUNCATE TABLE {string.Join(", ", uniqueTables)} CASCADE";
            await context.Database.ExecuteSqlRawAsync(batchSql);
        }
    }

    public async ValueTask DisposeAsync()
    {
        ApiClient?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();
    }

    public async Task<T> WithServiceScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    public async Task WithServiceScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    public static Guid ExtractIdFromLocation(string locationHeader)
    {
        if (locationHeader.Contains("?id="))
        {
            var uri = new Uri(locationHeader, UriKind.RelativeOrAbsolute);
            var idParam = uri.Query.TrimStart('?').Split('&').FirstOrDefault(p => p.StartsWith("id="));
            if (idParam != null) return Guid.Parse(idParam.Split('=')[1]);
        }
        return Guid.Parse(locationHeader.Split('/')[^1]);
    }

    public static void AuthenticateAsAdmin()
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    public static void AuthenticateAsUser(string userId = "test-user-id", string username = "testuser")
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userId, username);
    }

    public static void AuthenticateAsAnonymous()
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    public static void BeforeEachTest()
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    public async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PostAsync(requestUri, stringContent);
    }

    public async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PutAsync(requestUri, stringContent);
    }

    public async Task<HttpResponseMessage> PatchJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PatchAsync(requestUri, stringContent);
    }

    public static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    public async Task<Guid> CreateTestUserAsync(string? username = null, string? email = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            Username = username ?? $"testuser_{uniqueId}",
            Email = email ?? $"testuser_{uniqueId}@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Test@123456",
            PhoneNumber = "+5511999999999"
        };

        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions);
        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Failed to create test user. Status: {response.StatusCode}");
        }

        var locationHeader = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            throw new InvalidOperationException("Location header not found in create user response");
        }

        return ExtractIdFromLocation(locationHeader);
    }
}
