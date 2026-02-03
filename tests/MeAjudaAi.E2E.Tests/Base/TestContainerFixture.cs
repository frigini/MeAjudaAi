using DotNet.Testcontainers.Builders;
using Bogus;
using MeAjudaAi.ApiService;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Serialization;
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
    private PostgreSqlContainer _postgresContainer = null!;
    private RedisContainer _redisContainer = null!;
    private AzuriteContainer _azuriteContainer = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;
    public string RedisConnectionString { get; private set; } = null!;
    public string AzuriteConnectionString { get; private set; } = null!;
    public Faker Faker { get; } = new();

    public static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public async ValueTask InitializeAsync()
    {
        // Lógica de retry com exponential backoff para lidar com falhas transitórias do Docker
        const int maxRetries = 3;
        const int baseDelayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await InitializeContainersAsync();
                await InitializeFactoryAsync();
                return; // Sucesso
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
        // Configurar containers com timeouts aumentados para WSL2/Docker Desktop (Windows)
        _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
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
                        ["ConnectionStrings:SearchProvidersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:Redis"] = RedisConnectionString,
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
                        ["Cache:Enabled"] = "true", // Enable cache for realistic E2E testing
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

        // Create API client com handler que propaga contexto de teste
        var contextPropagationHandler = new TestContextAwareHandler
        {
            InnerHandler = _factory.Server.CreateHandler()
        };
        
        ApiClient = new HttpClient(contextPropagationHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };
        
        Services = _factory.Services;

        // Aplicar migrations e seed inicial
        await ApplyMigrationsAsync();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        // CRITICAL: Substituir autenticação real por ConfigurableTestAuthenticationHandler
        // NÃO remover services de autenticação - apenas substituir o scheme padrão
        
        // Adicionar/substituir test authentication com ConfigurableTestAuthenticationHandler
        services.AddAuthentication(MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.SchemeName)
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler>(
                MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.SchemeName,
                _ => { });
        
        // Garantir que authorization está configurado
        if (!services.Any(d => d.ServiceType == typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService)))
        {
            services.AddAuthorization();
        }
        
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
        ReconfigureDbContext<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>(services);

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
                npgsqlOptions.CommandTimeout(120); // 2 minutos timeout para queries (WSL2 overhead)
            });
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);

            // Suprimir warning de pending model changes em testes E2E
            // Migrations são aplicadas em runtime e podem estar ligeiramente desatualizadas
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
            // Aplicar migrations para cada módulo
            await ApplyMigrationForContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>(services);
            await ApplyMigrationForContext<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>(services);

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

    /// <summary>
    /// Executa ação com scope de serviço para acesso direto ao banco
    /// </summary>
    public async Task<T> WithServiceScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Executa ação com scope de serviço para acesso direto ao banco
    /// </summary>
    public async Task WithServiceScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Extrai o ID de um recurso do header Location de uma resposta HTTP 201 Created
    /// </summary>
    public static Guid ExtractIdFromLocation(string locationHeader)
    {
        if (locationHeader.Contains("?id="))
        {
            var uri = new Uri(locationHeader, UriKind.RelativeOrAbsolute);
            var query = uri.Query.TrimStart('?');
            var idParam = query.Split('&').FirstOrDefault(p => p.StartsWith("id="));

            if (idParam != null)
            {
                var idValue = idParam.Split('=')[1];
                return Guid.Parse(idValue);
            }
        }

        var segments = locationHeader.Split('/');
        return Guid.Parse(segments[^1]);
    }

    /// <summary>
    /// Configura autenticação como administrador.
    /// Cria/reut iliza contexto AsyncLocal automaticamente.
    /// </summary>
    public static void AuthenticateAsAdmin()
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    /// <summary>
    /// Configura autenticação como usuário regular.
    /// Cria/reutiliza contexto AsyncLocal automaticamente.
    /// </summary>
    public static void AuthenticateAsUser(string userId = "test-user-id", string username = "testuser")
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userId, username);
    }

    /// <summary>
    /// Remove autenticação (testes anônimos).
    /// </summary>
    public static void AuthenticateAsAnonymous()
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    /// <summary>
    /// Limpa o estado de autenticação. 
    /// IMPORTANTE: Chamar no início de cada teste para evitar vazamento de estado entre testes que compartilham fixture.
    /// </summary>
    public static void BeforeEachTest()
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

#pragma warning disable CA2000 // Dispose objects before losing scope - StringContent is disposed by HttpClient
    /// <summary>
    /// Envia POST com JSON serializado
    /// </summary>
    public async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PostAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Envia PUT com JSON serializado
    /// </summary>
    public async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PutAsync(requestUri, stringContent);
    }
#pragma warning restore CA2000

    /// <summary>
    /// Envia PATCH com JSON serializado
    /// </summary>
    public async Task<HttpResponseMessage> PatchJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PatchAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Deserializa JSON da resposta HTTP
    /// </summary>
    public static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Cria um usuário de teste e retorna seu ID
    /// </summary>
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
