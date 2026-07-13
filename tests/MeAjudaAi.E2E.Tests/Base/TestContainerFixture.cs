using Bogus;
using MeAjudaAi.ApiService;
using MeAjudaAi.E2E.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Messaging;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.E2E;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Documents;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Payments;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
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
    private static readonly SemaphoreSlim _initializationLock = new(1, 1);
    private static bool _containersInitialized = false;
    private static bool _migrationsApplied = false;

    // Two separate factories: one for events-enabled tests, one for non-events tests.
    // This prevents the static factory sharing issue where the first fixture's
    // EnableEventsAndMessageBus setting was captured permanently.
    private static bool _factoryEventsInitialized = false;
    private static bool _factoryNoEventsInitialized = false;
    private static WebApplicationFactory<Program>? _factoryEvents;
    private static WebApplicationFactory<Program>? _factoryNoEvents;
    private static HttpClient? _sharedApiClientEvents;
    private static HttpClient? _sharedApiClientNoEvents;
    private static IServiceProvider? _sharedServicesEvents;
    private static IServiceProvider? _sharedServicesNoEvents;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    public string PostgresConnectionString { get; private set; } = null!;
    public string RedisConnectionString { get; private set; } = null!;
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

        // Initialize the correct factory variant based on EnableEventsAndMessageBus.
        // Two separate factories prevent the static factory sharing issue where
        // the first fixture's config was captured permanently.
        var eventsEnabled = EnableEventsAndMessageBus;
        var factoryInitialized = eventsEnabled ? _factoryEventsInitialized : _factoryNoEventsInitialized;
        if (!factoryInitialized)
        {
            await _initializationLock.WaitAsync();
            try
            {
                factoryInitialized = eventsEnabled ? _factoryEventsInitialized : _factoryNoEventsInitialized;
                if (!factoryInitialized)
                {
                    await InitializeFactoryAsync(eventsEnabled);
                    if (eventsEnabled)
                        _factoryEventsInitialized = true;
                    else
                        _factoryNoEventsInitialized = true;
                }
            }
            finally
            {
                _initializationLock.Release();
            }
        }
        else
        {
            // Reuse properties already set by the one-time initialization
            if (eventsEnabled)
            {
                ApiClient = _sharedApiClientEvents!;
                Services = _sharedServicesEvents!;
            }
            else
            {
                ApiClient = _sharedApiClientNoEvents!;
                Services = _sharedServicesNoEvents!;
            }
        }

        // One-time migration application for the entire run (shared by both factories)
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
        _postgresContainer ??= new PostgreSqlBuilder("postgis/postgis:16-3.4")
                .WithDatabase("meajudaai_test")
                .WithUsername("postgres")
                .WithPassword("test123")
                .WithCleanUp(true)
                .Build();

        _redisContainer ??= new RedisBuilder("redis:7-alpine")
                .WithCleanUp(true)
                .Build();

        // Iniciar containers em paralelo
        var startTasks = new List<Task>();
        startTasks.Add(_postgresContainer.StartAsync());
        startTasks.Add(_redisContainer.StartAsync());

        await Task.WhenAll(startTasks);

        // Armazenar connection strings
        PostgresConnectionString = _postgresContainer.GetConnectionString();
        RedisConnectionString = _redisContainer.GetConnectionString();

        Console.WriteLine("✅ TestContainers initialized successfully");
    }

    private Task InitializeFactoryAsync(bool eventsEnabled)
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

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
                        ["Postgres:ConnectionString"] = PostgresConnectionString,
                        ["ConnectionStrings:Redis"] = RedisConnectionString,
                        ["Migrations:Enabled"] = "false",
                        ["Azure:Storage:ConnectionString"] = "UseDevelopmentStorage=true",
                        ["Hangfire:Enabled"] = "false",
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft"] = "Error",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Keycloak:Enabled"] = "false",
                        ["Keycloak:ClientSecret"] = "test-secret",
                        ["Keycloak:AdminUsername"] = "test-admin",
                        ["Keycloak:AdminPassword"] = "test-password",
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
                        ["RateLimit:WindowInSeconds"] = "3600",
                        ["GeographicRestriction:Enabled"] = "false",
                        ["GeographicRestriction:FailOpen"] = "true",
                        ["FeatureManagement:GeographicRestriction"] = "false",
                        // Stripe webhook configuration for test environment
                        ["Stripe:WebhookSecret"] = "whsec_test_mock_secret_for_testing"
                    });

                    config.AddEnvironmentVariables("MEAJUDAAI_TEST_");
                });

                // Use ConfigureTestServices for reliable overrides
                builder.ConfigureTestServices(services =>
                {
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Information);
                    });

                    ConfigureMockServices(services, eventsEnabled);
                    ReconfigureDbContexts(services);
                });
            });

        var contextPropagationHandler = new TestContextAwareHandler
        {
            InnerHandler = factory.Server.CreateHandler()
        };

        var apiClient = new HttpClient(contextPropagationHandler)
        {
            BaseAddress = new Uri("http://localhost")
        };

        // Cache for the correct variant
        if (eventsEnabled)
        {
            _factoryEvents = factory;
            _sharedApiClientEvents = apiClient;
            _sharedServicesEvents = factory.Services;
        }
        else
        {
            _factoryNoEvents = factory;
            _sharedApiClientNoEvents = apiClient;
            _sharedServicesNoEvents = factory.Services;
        }

        ApiClient = apiClient;
        Services = factory.Services;

        return Task.CompletedTask;
    }

    private void ConfigureMockServices(IServiceCollection services, bool eventsEnabled)
    {
        services.AddAuthentication(ConfigurableTestAuthenticationHandler.SchemeName)
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                ConfigurableTestAuthenticationHandler>(
               ConfigurableTestAuthenticationHandler.SchemeName,
                _ => { });
        
        if (!services.Any(d => d.ServiceType == typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService)))
        {
            services.AddAuthorization();
        }
        
        var keycloakDescriptors = services.Where(d => d.ServiceType == typeof(IKeycloakService)).ToList();
        foreach (var d in keycloakDescriptors) services.Remove(d);
        services.AddSingleton<IKeycloakService, MockKeycloakService>();

        var userDomainDescriptors = services.Where(d => d.ServiceType == typeof(IUserDomainService)).ToList();
        foreach (var d in userDomainDescriptors) services.Remove(d);
        services.AddScoped<IUserDomainService, MockUserDomainService>();

        var blobDescriptors = services.Where(d => d.ServiceType == typeof(IBlobStorageService)).ToList();
        foreach (var d in blobDescriptors) services.Remove(d);
        services.AddSingleton<IBlobStorageService, MockBlobStorageService>();

        var ocrDescriptors = services.Where(d => d.ServiceType == typeof(IDocumentIntelligenceService)).ToList();
        foreach (var d in ocrDescriptors) services.Remove(d);
        services.AddSingleton<IDocumentIntelligenceService, MockDocumentIntelligenceService>();

        var gatewayDescriptors = services.Where(d => d.ServiceType == typeof(IPaymentGateway)).ToList();
        foreach (var d in gatewayDescriptors) services.Remove(d);
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();

        // Register dummy Stripe client to satisfy DI validation
        services.AddSingleton<Stripe.IStripeClient>(new Stripe.StripeClient("sk_test_dummy"));

        // Message Bus: register based on eventsEnabled parameter.
        // Two separate factories are maintained (events vs non-events) to avoid
        // the static factory sharing issue where the first fixture's config was used for all.
        var busDescriptors = services.Where(d => d.ServiceType == typeof(IMessageBus)).ToList();
        foreach (var d in busDescriptors) services.Remove(d);

        var domainProcessorDescriptors = services.Where(d => d.ServiceType == typeof(IDomainEventProcessor)).ToList();
        foreach (var d in domainProcessorDescriptors) services.Remove(d);

        if (eventsEnabled)
        {
            services.AddSingleton<IMessageBus, FakeSynchronousMessageBus>();
            services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();
        }
        else
        {
            services.AddSingleton<IMessageBus, MockNoOpMessageBus>();
            services.AddScoped<IDomainEventProcessor, MockNoOpDomainEventProcessor>();
        }
    }

    private void ReconfigureDbContexts(IServiceCollection services)
    {
        ReconfigureDbContextWithUnitOfWork<UsersDbContext>(services, ModuleKeys.Users);
        ReconfigureDbContextWithUnitOfWork<ProvidersDbContext>(services, ModuleKeys.Providers);
        ReconfigureDbContextWithUnitOfWork<BookingsDbContext>(services, ModuleKeys.Bookings);
        ReconfigureDbContextWithUnitOfWork<DocumentsDbContext>(services, ModuleKeys.Documents);
        ReconfigureDbContextWithUnitOfWork<ServiceCatalogsDbContext>(services, ModuleKeys.ServiceCatalogs);
        services.AddScoped<IServiceCategoryQueries, DbContextServiceCategoryQueries>();
        services.AddScoped<IServiceQueries, DbContextServiceQueries>();
        ReconfigureDbContextWithUnitOfWork<LocationsDbContext>(services, ModuleKeys.Locations);
        ReconfigureDbContextWithUnitOfWork<CommunicationsDbContext>(services, ModuleKeys.Communications);
        ReconfigureDbContextWithUnitOfWork<SearchProvidersDbContext>(services, ModuleKeys.SearchProviders);
        ReconfigureDbContextWithUnitOfWork<RatingsDbContext>(services, ModuleKeys.Ratings);
        ReconfigureDbContextWithUnitOfWork<PaymentsDbContext>(services, ModuleKeys.Payments);

        // Remove ALL non-keyed IUnitOfWork registrations (production modules each register their own,
        // but only the last one wins in MS DI — Payments. This causes handlers to use the wrong DbContext.)
        var nonKeyedUowDescriptors = services.Where(d =>
            d.ServiceType == typeof(IUnitOfWork) && d.ServiceKey == null).ToList();
        foreach (var descriptor in nonKeyedUowDescriptors)
            services.Remove(descriptor);

        // Register a CompositeUnitOfWork as the single non-keyed IUnitOfWork.
        // It resolves the correct DbContext per aggregate by finding which registered DbContext
        // implements IRepository<TAggregate, TKey>.
        services.AddScoped<IUnitOfWork>(sp =>
            new CompositeTestUnitOfWork(sp));

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

    private void ReconfigureDbContextWithUnitOfWork<TContext>(IServiceCollection services, string moduleKey)
        where TContext : DbContext
    {
        ReconfigureDbContext<TContext>(services);

        // Registra o serviço por chave para o módulo usando o tipo do contexto diretamente
        services.AddKeyedScoped<IUnitOfWork>(moduleKey, (sp, key) => (IUnitOfWork)sp.GetRequiredService<TContext>());
    }

    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        // Console.WriteLine($"[DEBUG] Reconfiguring DbContext: {contextName}");

        var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (optionsDescriptor != null)
            services.Remove(optionsDescriptor);

        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor != null)
            services.Remove(contextDescriptor);

        services.AddDbContext<TContext>((sp, options) =>
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
        // Use either factory — both share the same database
        var factory = _factoryEvents ?? _factoryNoEvents;
        if (factory == null)
        {
            throw new InvalidOperationException("WebApplicationFactory was not initialized. This should not happen as factory initialization must complete before migrations are applied.");
        }

        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            await services.ApplyAllDiscoveredMigrationsAsync();

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
        var sp = scope.ServiceProvider;

        await sp.CleanupContextAsync<UsersDbContext>();
        await sp.CleanupContextAsync<ProvidersDbContext>();
        await sp.CleanupContextAsync<DocumentsDbContext>();
        await sp.CleanupContextAsync<ServiceCatalogsDbContext>();
        await sp.CleanupContextAsync<LocationsDbContext>();
        await sp.CleanupContextAsync<CommunicationsDbContext>();
        await sp.CleanupContextAsync<SearchProvidersDbContext>();
        await sp.CleanupContextAsync<RatingsDbContext>();
        await sp.CleanupContextAsync<PaymentsDbContext>();
        await sp.CleanupContextAsync<BookingsDbContext>();

        if (_redisContainer != null)
        {
            await _redisContainer.ExecAsync(new[] { "redis-cli", "FLUSHALL" });
        }
    }

    public async ValueTask DisposeAsync()
    {
        // NOTE: Do NOT dispose _factory or _sharedApiClient here.
        // _factory is static and shared across all fixture instances; disposing it
        // while another test class is still using it causes ObjectDisposedException.
        // The process will clean up static resources on exit.
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

        var lastSegment = locationHeader.Split('/')[^1].Split('?')[0];
        if (Guid.TryParse(lastSegment, out var id))
            return id;

        throw new FormatException($"Cannot extract GUID from Location header: {locationHeader}");
    }

    public static void AuthenticateAsAdmin()
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    public static void AuthenticateAsUser(string userId = "test-user-id", string username = "testuser")
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userId, username);
    }

    public static void AuthenticateAsAnonymous()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    /// <summary>
    /// Configura o contexto de autenticação para um usuário que é simultaneamente administrador do sistema 
    /// e vinculado a um prestador específico. Usa ConfigureProvider internamente com isSystemAdmin: true.
    /// </summary>
    /// <param name="providerId">O identificador do prestador ao qual o usuário será vinculado.</param>
    public static void AuthenticateAsAdminWithProvider(Guid providerId)
    {
        ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureProvider(
            providerId: providerId,
            userId: "admin-provider-id",
            username: "admin-provider",
            email: "admin@test.com",
            isSystemAdmin: true);
    }

    public static void BeforeEachTest()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    /// <summary>
    /// Extrai o objeto de dados de uma resposta JSON, suportando formatos { "value": {...} }, { "data": {...} } ou objeto direto.
    /// </summary>
    public static JsonElement GetResponseData(JsonElement response)
        => response.GetResponseData();

    public async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
        => await ApiClient.PostJsonAsync(requestUri, content);

    public async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
        => await ApiClient.PutJsonAsync(requestUri, content);

    public async Task<HttpResponseMessage> PatchJsonAsync<T>(string requestUri, T content)
        => await ApiClient.PatchJsonAsync(requestUri, content);

    public static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
        => await response.ReadJsonAsync<T>();

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

    /// <summary>
    /// Cria um prestador de testes via endpoint admin. Centralizado para evitar duplicação entre suítes E2E.
    /// </summary>
    public async Task<Guid> CreateTestProviderAsync(Guid userId, string? name = null)
    {
        var providerName = name ?? $"ProviderX_{Guid.NewGuid():N}";
        var request = new
        {
            UserId = userId.ToString(),
            Name = providerName,
            Type = 0, // EProviderType.Individual
            BusinessProfile = new
            {
                LegalName = providerName,
                FantasyName = providerName,
                Description = $"Test provider {providerName}",
                ContactInfo = new
                {
                    Email = $"{Guid.NewGuid():N}@example.com",
                    PhoneNumber = "+5511999999999"
                },
                PrimaryAddress = new
                {
                    Street = "Avenida Paulista",
                    Number = "1578",
                    Neighborhood = "Bela Vista",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01310-200",
                    Country = "Brasil"
                }
            }
        };

        var response = await ApiClient.PostAsJsonAsync("/api/v1/providers", request, JsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create test provider. Status: {response.StatusCode}, Body: {body}");
        }

        return ExtractIdFromLocation(response.Headers.Location!.ToString());
    }

    /// <summary>
    /// Configura o contexto de autenticação como um prestador específico.
    /// </summary>
    public static void AuthenticateAsProvider(Guid providerId, string? userId = null)
    {
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers.ConfigurableTestAuthenticationHandler.ConfigureProvider(
            providerId, userId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Insere um provider diretamente na tabela searchable_providers via Dapper.
    /// Necessário porque o MockMessageBus não processa eventos de integração.
    /// </summary>
    public async Task InsertSearchableProviderAsync(
        Guid providerId, string name, double latitude, double longitude,
        string city = "São Paulo", string state = "SP",
        decimal averageRating = 0m, int totalReviews = 0,
        int subscriptionTier = 1, Guid[]? serviceIds = null)
    {
        await WithServiceScopeAsync(async sp =>
        {
            var dapper = sp.GetRequiredService<IDapperConnection>();

            var sql = @"
                INSERT INTO search_providers.searchable_providers 
                (id, provider_id, slug, name, description, city, state, location, average_rating, total_reviews, subscription_tier, service_ids, is_active, created_at, updated_at)
                VALUES 
                (@Id, @ProviderId, @Slug, @Name, @Description, @City, @State, ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326)::geography, @AvgRating, @TotalReviews, @SubscriptionTier, @ServiceIds, @IsActive, @CreatedAt, @UpdatedAt)
                ON CONFLICT (provider_id) 
                DO UPDATE SET 
                    slug = EXCLUDED.slug,
                    name = EXCLUDED.name,
                    location = EXCLUDED.location,
                    average_rating = EXCLUDED.average_rating,
                    total_reviews = EXCLUDED.total_reviews,
                    service_ids = EXCLUDED.service_ids,
                    is_active = EXCLUDED.is_active,
                    updated_at = CURRENT_TIMESTAMP";

            await dapper.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                Slug = name.ToLowerInvariant().Replace(" ", "-").Replace("_", "-"),
                Name = name,
                Description = $"Test Provider {name}",
                City = city,
                State = state,
                Latitude = latitude,
                Longitude = longitude,
                AvgRating = averageRating,
                TotalReviews = totalReviews,
                SubscriptionTier = subscriptionTier,
                ServiceIds = serviceIds ?? Array.Empty<Guid>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        });
    }

    /// <summary>
    /// Cria uma categoria + serviço via API REST. Retorna o ID do serviço criado.
    /// </summary>
    public async Task<Guid> CreateTestServiceViaApiAsync(
        string? categoryName = null, string? serviceName = null)
    {
        AuthenticateAsAdmin();

        categoryName ??= $"Category_{Guid.NewGuid():N}";
        var catResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories",
            new { name = categoryName, displayOrder = 1 }, JsonOptions);
        catResponse.EnsureSuccessStatusCode();
        var catId = ExtractIdFromLocation(catResponse.Headers.Location!.ToString());

        serviceName ??= $"Service_{Guid.NewGuid():N}";
        var svcResponse = await ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services",
            new { name = serviceName, categoryId = catId }, JsonOptions);
        svcResponse.EnsureSuccessStatusCode();
        return ExtractIdFromLocation(svcResponse.Headers.Location!.ToString());
    }

    /// <summary>
    /// Vincula um serviço a um provedor via API REST (POST /api/v1/providers/{id}/services/{serviceId}).
    /// </summary>
    public async Task LinkServiceToProviderAsync(Guid providerId, Guid serviceId)
    {
        var response = await ApiClient.PostAsync($"/api/v1/providers/{providerId}/services/{serviceId}", null);
        response.EnsureSuccessStatusCode();
    }
}