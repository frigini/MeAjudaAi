using Bogus;
using MeAjudaAi.ApiService;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using DotNet.Testcontainers.Configurations;
using MeAjudaAi.E2E.Tests.Base.Helpers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using System.Net.Http.Json;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Fixture compartilhada para testes E2E usando TestContainers.
/// Implementa IClassFixture para compartilhar containers entre testes da mesma classe.
/// Reduz overhead de criação de containers.
/// </summary>
public class TestContainerFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim _migrationLock = new(1, 1);
    private static bool _migrationsApplied = false;

    private WebApplicationFactory<Program> _factory = null!;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    public string PostgresConnectionString => SharedTestContainers.PostgresConnectionString;
    public string RedisConnectionString => SharedTestContainers.RedisConnectionString;
    public string AzuriteConnectionString => SharedTestContainers.AzuriteConnectionString;
    public Faker Faker { get; } = new();

    /// <summary>
    /// Define se o sistema de mensagens síncrono e processamento de eventos devem estar ativos.
    /// Por padrão é falso para isolar testes unitários/E2E simples.
    /// </summary>
    public virtual bool EnableEventsAndMessageBus => false;

    public static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public async ValueTask InitializeAsync()
    {
        var diagPath = Path.Combine(AppContext.BaseDirectory, "fixture_diag.log");
        File.WriteAllText(diagPath, $"[{DateTime.Now}] InitializeAsync starting...\n");
        
        // One-time initialization for the entire test run
        await SharedTestContainers.EnsureInitializedAsync();
        File.AppendAllText(diagPath, $"[{DateTime.Now}] Shared containers initialized.\n");
        
        // One-time migration application for the entire test run
        if (!_migrationsApplied)
        {
            File.AppendAllText(diagPath, $"[{DateTime.Now}] Waiting for migration lock...\n");
            await _migrationLock.WaitAsync();
            try
            {
                if (!_migrationsApplied)
                {
                    File.AppendAllText(diagPath, $"[{DateTime.Now}] Applying migrations...\n");
                    await ApplyMigrationsAsync();
                    _migrationsApplied = true;
                    File.AppendAllText(diagPath, $"[{DateTime.Now}] Migrations applied successfully.\n");
                }
            }
            finally
            {
                _migrationLock.Release();
            }
        }

        // Initialize WebApplicationFactory for THIS test class instance
        await InitializeFactoryAsync();
        File.AppendAllText(diagPath, $"[{DateTime.Now}] Factory initialized.\n");
    }

    private async Task InitializeFactoryAsync()
    {
#pragma warning disable CA2000 // Dispose é gerenciado por IAsyncLifetime.DisposeAsync
        _factory = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = PostgresConnectionString,
                        ["ConnectionStrings:meajudaai-db"] = PostgresConnectionString,
                        ["ConnectionStrings:UsersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:ProvidersDb"] = PostgresConnectionString,
                        ["ConnectionStrings:RatingsDb"] = PostgresConnectionString,
                        ["ConnectionStrings:DocumentsDb"] = PostgresConnectionString,
                        ["ConnectionStrings:Redis"] = RedisConnectionString,
                        ["Azure:Storage:ConnectionString"] = AzuriteConnectionString,
                        ["Hangfire:Enabled"] = "false",
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft"] = "Error",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Keycloak:Enabled"] = "false",
                        ["ExternalServices:Keycloak:Enabled"] = "false",
                        ["ExternalServices:PaymentGateway:Enabled"] = "false",
                        ["ExternalServices:Geolocation:Enabled"] = "false",
                        ["Cache:Enabled"] = "false",
                        ["Cache:ConnectionString"] = RedisConnectionString,
                        ["AdvancedRateLimit:General:Enabled"] = "false",
                        ["AdvancedRateLimit:General:EnableIpWhitelist"] = "true",
                        ["RateLimit:DefaultRequestsPerMinute"] = "999999",
                        ["RateLimit:WindowInSeconds"] = "3600"
                    });

                    config.AddEnvironmentVariables("MEAJUDAAI_TEST_");
                });

                builder.ConfigureServices((context, services) =>
                {
                    // Remover background workers que interferem com migrations e isolamento
                    var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
                    foreach (var service in hostedServices)
                    {
                        services.Remove(service);
                    }

                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Error);
                    });

                    var configuration = context.Configuration;
                    var environment = context.HostingEnvironment;

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
        services.AddAuthentication(ConfigurableTestAuthenticationHandler.SchemeName)
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                ConfigurableTestAuthenticationHandler>(
                ConfigurableTestAuthenticationHandler.SchemeName,
                _ => { });
        
        if (!services.Any(d => d.ServiceType == typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService)))
        {
            services.AddAuthorization();
        }

        // Mocks for external dependencies
        ReplaceService<IKeycloakService, MockKeycloakService>(services, ServiceLifetime.Scoped);
        ReplaceService<MeAjudaAi.Modules.Users.Domain.Services.IUserDomainService, MockUserDomainService>(services, ServiceLifetime.Scoped);
        ReplaceService<IBlobStorageService, MockBlobStorageService>(services, ServiceLifetime.Scoped);

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

        // SearchProviders specific metrics/dapper
        var metricsType = typeof(MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext).Assembly
            .GetType("MeAjudaAi.Modules.SearchProviders.Infrastructure.Monitoring.DatabaseMetrics");
        if (metricsType != null && !services.Any(d => d.ServiceType == metricsType))
        {
            services.AddSingleton(metricsType);
        }

        var dapperInterface = typeof(MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider).Assembly
            .GetType("MeAjudaAi.Modules.SearchProviders.Domain.Interfaces.IDapperConnection");
        var dapperImpl = typeof(MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext).Assembly
            .GetType("MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.DapperConnection");
            
        if (dapperInterface != null && dapperImpl != null)
        {
            var dapperDescriptor = services.SingleOrDefault(d => d.ServiceType == dapperInterface);
            if (dapperDescriptor != null) services.Remove(dapperDescriptor);
            services.AddScoped(dapperInterface, dapperImpl);
        }
    }

    private void ReconfigureDbContexts(IServiceCollection services)
    {
        ReconfigureDbContext<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.BookingsDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>(services);
        ReconfigureDbContext<MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext>(services);

        var postgresOptionsDescriptors = services.Where(d => d.ServiceType == typeof(MeAjudaAi.Shared.Database.PostgresOptions)).ToList();
        foreach (var descriptor in postgresOptionsDescriptors)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton(new MeAjudaAi.Shared.Database.PostgresOptions
        {
            ConnectionString = PostgresConnectionString
        });
    }

    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(PostgresConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextSchemaHelper.GetSchemaName(contextName));
                npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
                
                if (typeof(TContext).Name.Contains("SearchProviders"))
                {
                    npgsqlOptions.UseNetTopologySuite();
                }
                
                npgsqlOptions.CommandTimeout(120);
            });
            options.UseSnakeCaseNamingConvention();
            options.EnableSensitiveDataLogging(false);
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
    }

    private async Task ApplyMigrationsAsync()
    {
        Console.WriteLine("🔄 [TestContainerFixture] Applying global migrations...");
        
        await ApplyIndependentMigration<MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.RatingsDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Locations.Infrastructure.Persistence.LocationsDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Communications.Infrastructure.Persistence.CommunicationsDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.BookingsDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.SearchProvidersDbContext>();
        await ApplyIndependentMigration<MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext>();

        Console.WriteLine("✅ [TestContainerFixture] All global migrations applied.");
    }

    private async Task ApplyIndependentMigration<TContext>() where TContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        var contextName = typeof(TContext).Name;
        
        optionsBuilder.UseNpgsql(SharedTestContainers.PostgresConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextSchemaHelper.GetSchemaName(contextName));
            npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
            
            if (typeof(TContext).Name.Contains("SearchProviders"))
            {
                npgsqlOptions.UseNetTopologySuite();
            }
        }).UseSnakeCaseNamingConvention()
        .ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        using var context = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
        await MigrationTestHelper.ApplyMigrationForContext(context);
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
        await CleanupContext<MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext>(services);

        await SharedTestContainers.FlushRedisAsync();
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
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create test user. Status: {response.StatusCode}. Error: {errorContent}");
        }

        var locationHeader = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            throw new InvalidOperationException("Location header not found in create user response");
        }

        return ExtractIdFromLocation(locationHeader);
    }

    private void ReplaceService<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime)
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
