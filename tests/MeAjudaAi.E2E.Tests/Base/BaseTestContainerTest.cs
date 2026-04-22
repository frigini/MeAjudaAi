using System.Net;
using Bogus;
using MeAjudaAi.ApiService;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.E2E.Tests.Base.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;


// Disable parallel execution to prevent race conditions when using shared database containers
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Base class para testes E2E usando TestContainers
/// Isolada de Aspire, com infraestrutura própria de teste.
/// Refatorado para usar Singleton Pattern nos containers para evitar exaustão de recursos.
/// </summary>
public abstract class BaseTestContainerTest : IAsyncLifetime
{
    // Static containers shared across all tests
    private static PostgreSqlContainer? _postgresContainer;
    private static RedisContainer? _redisContainer;
    private static AzuriteContainer? _azuriteContainer;

    /// <summary>
    /// Sobrescreva em classes derivadas para habilitar o message bus em memória síncrono e eventos de domínio.
    /// Usado para testes que dependem de eventos de integração entre módulos (ex: Ratings -> SearchProviders).
    /// </summary>
    protected virtual bool EnableEventsAndMessageBus => false;

    // Locking for thread-safe initialization
    private static readonly SemaphoreSlim _initializationLock = new(1, 1);
    private static bool _initialized = false;

    private WebApplicationFactory<Program> _factory = null!;

    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    protected static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public virtual async ValueTask InitializeAsync()
    {
        // Ensure containers are initialized only once
        if (!_initialized)
        {
            await _initializationLock.WaitAsync();
            try
            {
                if (!_initialized)
                {
                    // Configurar containers com configuração mais robusta
                    if (_postgresContainer == null)
                    {
                        // Enable legacy timestamp behavior immediately
                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

                    // Start containers in parallel
                    var tasks = new List<Task>();
                    tasks.Add(_postgresContainer.StartAsync());
                    tasks.Add(_redisContainer.StartAsync());
                    tasks.Add(_azuriteContainer.StartAsync());

                    await Task.WhenAll(tasks);

                    _initialized = true;
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        // Configurar WebApplicationFactory (instância por teste, mas usa containers compartilhados)
        InitializeFactory();

        // Create HTTP client with test context header injection
        ApiClient = _factory.CreateDefaultClient(new TestContextHeaderHandler());

        // Para a primeira execução, precisamos aplicar as migrações.
        // Como todos os testes compartilham o banco, aplicamos de forma idempotente.
        await ApplyMigrationsAsync();

        // Aguardar API ficar disponível
        await WaitForApiHealthAsync();
    }

    private void InitializeFactory()
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
                        // Todos os módulos compartilham a mesma instância de banco de dados de teste
                        ["ConnectionStrings:DefaultConnection"] = _postgresContainer!.GetConnectionString(),
                        ["ConnectionStrings:meajudaai-db"] = _postgresContainer!.GetConnectionString(),
                        ["ConnectionStrings:UsersDb"] = _postgresContainer!.GetConnectionString(),
                        ["ConnectionStrings:ProvidersDb"] = _postgresContainer!.GetConnectionString(),
                        ["ConnectionStrings:RatingsDb"] = _postgresContainer!.GetConnectionString(),
                        ["ConnectionStrings:DocumentsDb"] = _postgresContainer!.GetConnectionString(),
                        ["ConnectionStrings:Redis"] = _redisContainer!.GetConnectionString(),
                        ["Azure:Storage:ConnectionString"] = _azuriteContainer!.GetConnectionString(),
                        ["Hangfire:Enabled"] = "false", // Desabilitar Hangfire nos testes E2E
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft"] = "Error",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Keycloak:Enabled"] = "false",
                         // Desabilitar health checks de serviços externos nos testes E2E
                        ["ExternalServices:Keycloak:Enabled"] = "false",
                        ["ExternalServices:PaymentGateway:Enabled"] = "false",
                        ["ExternalServices:Geolocation:Enabled"] = "false",
                        ["Cache:Enabled"] = "false",
                        ["Cache:ConnectionString"] = _redisContainer!.GetConnectionString(),
                        // Desabilitar completamente Rate Limiting nos testes E2E
                        ["AdvancedRateLimit:General:Enabled"] = "false",
                         // Valores válidos caso não consiga desabilitar completamente
                        ["AdvancedRateLimit:Anonymous:RequestsPerMinute"] = "10000",
                        ["AdvancedRateLimit:Anonymous:RequestsPerHour"] = "100000",
                        ["AdvancedRateLimit:Anonymous:RequestsPerDay"] = "1000000",
                        ["AdvancedRateLimit:Authenticated:RequestsPerMinute"] = "10000",
                        ["AdvancedRateLimit:Authenticated:RequestsPerHour"] = "100000",
                        ["AdvancedRateLimit:Authenticated:RequestsPerDay"] = "1000000",
                        ["AdvancedRateLimit:General:WindowInSeconds"] = "60",
                        ["AdvancedRateLimit:General:EnableIpWhitelist"] = "true",
                        // Configuração legada também para garantir
                        ["RateLimit:DefaultRequestsPerMinute"] = "999999",
                        ["RateLimit:AuthRequestsPerMinute"] = "999999",
                        ["RateLimit:SearchRequestsPerMinute"] = "999999",
                        ["RateLimit:WindowInSeconds"] = "3600"
                    });

                    // Adicionar ambiente de teste
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

                    // Reconfigurar todos os DbContexts com TestContainer connection string
                    ReconfigureDbContext<UsersDbContext>(services);
                    ReconfigureDbContext<ProvidersDbContext>(services);
                    ReconfigureDbContext<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>(services);
                    ReconfigureDbContext<DocumentsDbContext>(services);
                    ReconfigureDbContext<ServiceCatalogsDbContext>(services);
                    ReconfigureDbContext<LocationsDbContext>(services);
                    ReconfigureDbContext<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>(services);
                    ReconfigureDbContext<BookingsDbContext>(services);
                    ReconfigureDbContext<SearchProvidersDbContext>(services);

                    // Configurar PostgresOptions e Dapper para SearchProviders
                    var postgresOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PostgresOptions));
                    if (postgresOptionsDescriptor != null)
                        services.Remove(postgresOptionsDescriptor);

                    services.AddSingleton(new PostgresOptions
                    {
                        ConnectionString = _postgresContainer!.GetConnectionString()
                    });

                    // Adicionar DatabaseMetrics se não existir
                    if (!services.Any(d => d.ServiceType == typeof(DatabaseMetrics)))
                    {
                        services.AddSingleton<DatabaseMetrics>();
                    }

                    // Adicionar DapperConnection para SearchProviders
                    var dapperDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDapperConnection));
                    if (dapperDescriptor != null)
                        services.Remove(dapperDescriptor);

                    services.AddScoped<IDapperConnection, DapperConnection>();

                    // Substituir IKeycloakService por MockKeycloakService para testes
                    var keycloakDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IKeycloakService));
                    if (keycloakDescriptor != null)
                        services.Remove(keycloakDescriptor);

                    services.AddScoped<IKeycloakService, MockKeycloakService>();

                    // Substituir IUserDomainService por MockUserDomainService para testes
                    var userDomainServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(MeAjudaAi.Modules.Users.Domain.Services.IUserDomainService));
                    if (userDomainServiceDescriptor != null)
                        services.Remove(userDomainServiceDescriptor);

                    services.AddScoped<MeAjudaAi.Modules.Users.Domain.Services.IUserDomainService, MockUserDomainService>();

                    // Substituir IBlobStorageService por MockBlobStorageService para testes
                    var blobStorageDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlobStorageService));
                    if (blobStorageDescriptor != null)
                        services.Remove(blobStorageDescriptor);

                    services.AddScoped<IBlobStorageService, MockBlobStorageService>();

                    if (EnableEventsAndMessageBus)
                    {
                        ReplaceService<MeAjudaAi.Shared.Messaging.IMessageBus, MeAjudaAi.E2E.Tests.Infrastructure.SynchronousInMemoryMessageBus>(services, ServiceLifetime.Singleton);
                        ReplaceService<MeAjudaAi.Shared.Events.IDomainEventProcessor, MeAjudaAi.Shared.Events.DomainEventProcessor>(services, ServiceLifetime.Scoped);
                    }
                    else
                    {
                        ReplaceService<MeAjudaAi.Shared.Messaging.IMessageBus, MockMessageBus>(services, ServiceLifetime.Singleton);
                        ReplaceService<MeAjudaAi.Shared.Events.IDomainEventProcessor, MockDomainEventProcessor>(services, ServiceLifetime.Scoped);
                    }

                    // Remove todas as configurações de autenticação existentes
                    var authDescriptors = services
                        .Where(d => d.ServiceType.Namespace?.Contains("Authentication") == true)
                        .ToList();
                    foreach (var authDescriptor in authDescriptors)
                    {
                        services.Remove(authDescriptor);
                    }

                    // Configura apenas autenticação de teste como esquema padrão
                    services.AddAuthentication(ConfigurableTestAuthenticationHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, ConfigurableTestAuthenticationHandler>(
                            ConfigurableTestAuthenticationHandler.SchemeName, options => { });

                     // Add simple default migration hooks
                    services.AddScoped<Func<UsersDbContext>>(provider => () =>
                    {
                        var context = provider.GetRequiredService<UsersDbContext>();
                        return context;
                    });

                    services.AddScoped<Func<ProvidersDbContext>>(provider => () =>
                    {
                        var context = provider.GetRequiredService<ProvidersDbContext>();
                        return context;
                    });
                });
            });
    }

    public virtual async ValueTask DisposeAsync()
    {
        // Clear authentication context to prevent state pollution between tests
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        ApiClient?.Dispose();
        _factory?.Dispose();

        // DO NOT dispose containers here as they are shared static singletons
        // They will be cleaned up by Ryuk (Testcontainers) when the process exits
        await Task.CompletedTask;
    }

    private async Task WaitForApiHealthAsync()
    {
        const int maxAttempts = 15;
        const int delayMs = 2000;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var response = await ApiClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                // Continue to next attempt
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException($"API did not become healthy after {maxAttempts} attempts");
    }

    // Static flag for database migration state
    private static bool _migrationsApplied = false;
    private static readonly SemaphoreSlim _migrationLock = new(1, 1);

    private async Task ApplyMigrationsAsync()
    {
        if (_migrationsApplied) return;

        await _migrationLock.WaitAsync();
        try
        {
            if (_migrationsApplied) return;

            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Apply migrations for all DbContexts using helper
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<UsersDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<ServiceCatalogsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<ProvidersDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<RatingsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<DocumentsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<LocationsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<CommunicationsDbContext>());
            await MigrationTestHelper.ApplyMigrationForContext(services.GetRequiredService<BookingsDbContext>());

            // 6. SearchProviders (Depends on Providers + PostGIS)
            var searchContext = scope.ServiceProvider.GetService<SearchProvidersDbContext>();
            if (searchContext != null)
            {
                await MigrationTestHelper.ApplyMigrationForContext(searchContext);
            }

            _migrationsApplied = true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to apply migrations: {ex.Message}", ex);
        }
        finally
        {
            _migrationLock.Release();
        }
    }

    /// <summary>
    /// Limpa todas as tabelas de todos os contextos registrados e o cache do Redis para garantir isolamento entre testes.
    /// </summary>
    /// <remarks>
    /// Este método não é chamado automaticamente pelo <see cref="InitializeAsync"/> ou <see cref="DisposeAsync"/>.
    /// Deve ser invocado explicitamente por testes derivados quando o isolamento do teste exigir a limpeza do estado 
    /// do banco de dados e do Redis.
    /// <para>
    /// Uso típico: Chamada no início ou no final de um teste ou fixture, por exemplo: <c>await CleanupDatabaseAsync();</c>
    /// </para>
    /// <para>
    /// Efeitos colaterais: Limpa todos os <c>DbContexts</c> registrados via <see cref="CleanupContext{T}"/> 
    /// e executa um <c>FLUSHALL</c> no Redis se <c>_redisContainer</c> estiver disponível.
    /// </para>
    /// </remarks>
    protected async Task CleanupDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        await CleanupContext<UsersDbContext>(services);
        await CleanupContext<ProvidersDbContext>(services);
        await CleanupContext<DocumentsDbContext>(services);
        await CleanupContext<ServiceCatalogsDbContext>(services);
        await CleanupContext<LocationsDbContext>(services);
        await CleanupContext<CommunicationsDbContext>(services);
        await CleanupContext<BookingsDbContext>(services);
        await CleanupContext<SearchProvidersDbContext>(services);
        await CleanupContext<RatingsDbContext>(services);

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

    // Helper methods usando serialização compartilhada
#pragma warning disable CA2000 // Dispose StringContent - handled by HttpClient
    /// <summary>
    /// Envia uma requisição POST com conteúdo JSON para o URI especificado.
    /// </summary>
    /// <typeparam name="T">O tipo do conteúdo a ser serializado.</typeparam>
    /// <param name="requestUri">O URI para enviar a requisição.</param>
    /// <param name="content">O conteúdo a ser serializado e enviado.</param>
    /// <returns>A mensagem de resposta HTTP.</returns>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PostAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Envia uma requisição PUT com conteúdo JSON para o URI especificado.
    /// </summary>
    /// <typeparam name="T">O tipo do conteúdo a ser serializado.</typeparam>
    /// <param name="requestUri">O URI para enviar a requisição.</param>
    /// <param name="content">O conteúdo a ser serializado e enviado.</param>
    /// <returns>A mensagem de resposta HTTP.</returns>
    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PutAsync(requestUri, stringContent);
    }
#pragma warning restore CA2000

    /// <summary>
    /// Desserializa conteúdo JSON de uma resposta HTTP.
    /// </summary>
    /// <typeparam name="T">O tipo para desserializar.</typeparam>
    /// <param name="response">A resposta HTTP contendo conteúdo JSON.</param>
    /// <returns>O objeto desserializado, ou null se a desserialização falhar.</returns>
    protected static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Executa ação com scope de serviço para acesso direto ao banco
    /// </summary>
    protected async Task<T> WithServiceScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _factory.Services.CreateScope();
        return await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Executa ação com scope de serviço para acesso direto ao banco
    /// </summary>
    protected async Task WithServiceScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        await action(scope.ServiceProvider);
    }

    /// <summary>
    /// Executa ação com contexto do banco de dados
    /// </summary>
    protected async Task<T> WithDbContextAsync<T>(Func<UsersDbContext, Task<T>> action)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        return await action(context);
    }

    /// <summary>
    /// Executa ação com contexto do banco de dados
    /// </summary>
    protected async Task WithDbContextAsync(Func<UsersDbContext, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await action(context);
    }

    /// <summary>
    /// Configura autenticação como administrador para testes
    /// </summary>
    protected static void AuthenticateAsAdmin()
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    /// <summary>
    /// Configura autenticação como usuário regular para testes
    /// </summary>
    protected static void AuthenticateAsUser(string userId = "test-user-id", string username = "testuser")
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userId, username);
    }

    /// <summary>
    /// Remove autenticação (testes anônimos)
    /// </summary>
    protected static void AuthenticateAsAnonymous()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    /// <summary>
    /// Envia uma requisição POST com conteúdo JSON para o URI especificado.
    /// </summary>
    /// <typeparam name="T">O tipo do conteúdo a ser serializado.</typeparam>
    /// <param name="requestUri">O URI para enviar a requisição.</param>
    /// <param name="content">O conteúdo a ser serializado e enviado.</param>
    /// <returns>A mensagem de resposta HTTP.</returns>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T content)
        => await PostJsonAsync(requestUri.ToString(), content);

    /// <summary>
    /// Envia uma requisição PUT com conteúdo JSON para o URI especificado.
    /// </summary>
    /// <typeparam name="T">O tipo do conteúdo a ser serializado.</typeparam>
    /// <param name="requestUri">O URI para enviar a requisição.</param>
    /// <param name="content">O conteúdo a ser serializado e enviado.</param>
    /// <returns>A mensagem de resposta HTTP.</returns>
    protected async Task<HttpResponseMessage> PutJsonAsync<T>(Uri requestUri, T content)
        => await PutJsonAsync(requestUri.ToString(), content);

    /// <summary>
    /// Reconfigura um DbContext para usar a connection string do TestContainer
    /// </summary>
    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(_postgresContainer!.GetConnectionString(), npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextSchemaHelper.GetSchemaName(contextName));
                npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
                
                // Apenas SearchProviders requer NetTopologySuite (PostGIS)
                if (typeof(TContext) == typeof(SearchProvidersDbContext))
                {
                    npgsqlOptions.UseNetTopologySuite();
                }
                
                npgsqlOptions.CommandTimeout(120);
            })
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(true)
            .ConfigureWarnings(warnings =>
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });
            }

    /// <summary>
    /// Extrai o ID de um recurso do header Location de uma resposta HTTP 201 Created.
    /// Suporta formatos: /api/v1/resource/{id}, /api/v1/resource?id={id}
    /// </summary>
    protected static Guid ExtractIdFromLocation(string locationHeader)
    {
        if (locationHeader.Contains("?id="))
        {
            var queryString = locationHeader.Split('?')[1];
            var idParam = queryString.Split('&')
                .FirstOrDefault(p => p.StartsWith("id="));

            if (idParam != null)
            {
                var idValue = idParam.Split('=')[1];
                return Guid.Parse(idValue);
            }
        }

        var segments = locationHeader.Split('/');
        var lastSegment = segments[^1].Split('?')[0];
        return Guid.Parse(lastSegment);
    }

    /// <summary>
    /// Cria um usuário de teste e retorna seu ID
    /// </summary>
    protected async Task<Guid> CreateTestUserAsync(string? username = null, string? email = null)
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
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create test user. Status: {response.StatusCode}, Body: {body}");
        }

        var locationHeader = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            throw new InvalidOperationException("Location header not found in create user response");
        }

        return ExtractIdFromLocation(locationHeader);
    }

    /// <summary>
    /// HTTP message handler that injects test context ID header into all requests
    /// </summary>
    private class TestContextHeaderHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Add test context ID header to isolate authentication between parallel tests
            var contextId = ConfigurableTestAuthenticationHandler.GetCurrentTestContextId();
            if (contextId != null)
            {
                request.Headers.Add(ConfigurableTestAuthenticationHandler.TestContextHeader, contextId);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

    /// <summary>
    /// Helper para substituir um serviço existente na IServiceCollection.
    /// </summary>
    protected static void ReplaceService<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime)
        where TImplementation : class, TService
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToList();
        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        var newDescriptor = new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime);
        services.Add(newDescriptor);
    }
}
