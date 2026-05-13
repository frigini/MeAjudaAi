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
using MeAjudaAi.Shared.Database;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Fixture compartilhada para testes E2E usando TestContainers.
/// Implementa IClassFixture para compartilhar containers entre testes da mesma classe.
/// Reduz overhead de criação de containers.
/// </summary>
public class TestContainerFixture : IAsyncLifetime
{
    private WebApplicationFactory<Program> _factory = null!;

    public static string PostgresConnectionString => SharedTestContainers.PostgresConnectionString;
    public static string RedisConnectionString => SharedTestContainers.RedisConnectionString;
    public static string AzuriteConnectionString => SharedTestContainers.AzuriteConnectionString;

    public HttpClient ApiClient { get; private set; } = null!;
    public IServiceProvider Services { get; private set; } = null!;
    public static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Default;

    public Faker Faker { get; } = new("pt_BR");

    public virtual bool EnableEventsAndMessageBus => false;

    public async ValueTask InitializeAsync()
    {
        var diagPath = Path.Combine(AppContext.BaseDirectory, "fixture_init.log");
        await AppendLogAsync(diagPath, "TestContainerFixture: InitializeAsync starting...");

        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
            Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

            await AppendLogAsync(diagPath, "Calling E2EStabilityCoordinator.EnsureInitializedAsync...");
            using var initCts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            await E2EStabilityCoordinator.EnsureInitializedAsync();
            await AppendLogAsync(diagPath, "E2EStabilityCoordinator.EnsureInitializedAsync completed.");

            await AppendLogAsync(diagPath, "Building WebApplicationFactory...");
            _factory = new WebApplicationFactory<Program>()
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
                                var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
                                foreach (var service in hostedServices) services.Remove(service);

                                services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Information));
                                services.AddScoped<MeAjudaAi.Modules.Locations.Application.Services.IGeocodingService, MeAjudaAi.E2E.Tests.Infrastructure.Mocks.MockGeocodingService>();
                                ConfigureMockServices(services);
                                ReconfigureDbContexts(services);
                            });
                        });
            await AppendLogAsync(diagPath, "WebApplicationFactory created.");

            var contextPropagationHandler = new TestContextAwareHandler
            {
                InnerHandler = _factory.Server.CreateHandler()
            };

            ApiClient = new HttpClient(contextPropagationHandler)
            {
                BaseAddress = new Uri("http://localhost")
            };

            Services = _factory.Services;

            await AppendLogAsync(diagPath, "Checking cleanup flag...");
            if (!"true".Equals(Environment.GetEnvironmentVariable("E2E_SKIP_LOCAL_CLEANUP"), StringComparison.OrdinalIgnoreCase))
            {
                await AppendLogAsync(diagPath, "Calling CleanupDatabaseAsync...");
                await CleanupDatabaseAsync();
                await AppendLogAsync(diagPath, "CleanupDatabaseAsync completed.");
            }

            await AppendLogAsync(diagPath, "TestContainerFixture: InitializeAsync completed successfully.");
        }
        catch (Exception ex)
        {
            var errorMsg = $"[FATAL] TestContainerFixture.InitializeAsync failed: {ex.Message}\n{ex.StackTrace}";
            await AppendLogAsync(diagPath, errorMsg);
            Console.Error.WriteLine(errorMsg);

            _factory?.Dispose();
            throw;
        }
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

        // Mocks para dependências externas
        ReplaceService<IKeycloakService, MockKeycloakService>(services, ServiceLifetime.Scoped);
        ReplaceService<MeAjudaAi.Modules.Users.Domain.Services.IUserDomainService, MockUserDomainService>(services, ServiceLifetime.Scoped);
        ReplaceService<IBlobStorageService, MockBlobStorageService>(services, ServiceLifetime.Scoped);
        ReplaceService<MeAjudaAi.Modules.Payments.Domain.Abstractions.IPaymentGateway, MeAjudaAi.E2E.Tests.Infrastructure.Mocks.MockPaymentGateway>(services, ServiceLifetime.Singleton);

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

        // Métricas e Dapper específicos do SearchProviders
        if (!services.Any(d => d.ServiceType == typeof(DatabaseMetrics)))
        {
            services.AddSingleton<DatabaseMetrics>();
        }

        var dapperDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDapperConnection));
        if (dapperDescriptor != null) services.Remove(dapperDescriptor);
        services.AddScoped<IDapperConnection, DapperConnection>();
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

        var postgresOptionsDescriptors = services.Where(d => d.ServiceType == typeof(PostgresOptions)).ToList();
        foreach (var descriptor in postgresOptionsDescriptors)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton(new PostgresOptions
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

    public async Task CleanupDatabaseAsync()
    {
        if ("true".Equals(Environment.GetEnvironmentVariable("E2E_SKIP_LOCAL_CLEANUP"), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var diagPath = Path.Combine(AppContext.BaseDirectory, "fixture_diag.log");
        await AppendLogAsync(diagPath, "CleanupDatabaseAsync starting...");
        await E2EStabilityCoordinator.GlobalCleanupAsync();
        await AppendLogAsync(diagPath, "GlobalCleanupAsync done, flushing Redis...");
        await SharedTestContainers.FlushRedisAsync();
        await AppendLogAsync(diagPath, "CleanupDatabaseAsync completed.");
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

    public ValueTask DisposeAsync()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ApiClient?.Dispose();
        _factory?.Dispose();
        return ValueTask.CompletedTask;
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

        var diagPath = Path.Combine(AppContext.BaseDirectory, "fixture_diag.log");
        await AppendLogAsync(diagPath, $"CreateTestUserAsync: Posting to /api/v1/users for {createRequest.Username}...");
        
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var response = await ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, JsonOptions, cts.Token);
        
        await AppendLogAsync(diagPath, $"CreateTestUserAsync response: {response.StatusCode}");
        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            await AppendLogAsync(diagPath, $"CreateTestUserAsync FAILED: {errorContent}");
            throw new InvalidOperationException($"Failed to create test user. Status: {response.StatusCode}. Error: {errorContent}");
        }

        var locationHeader = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
        {
            throw new InvalidOperationException("Location header not found in create user response");
        }

        return ExtractIdFromLocation(locationHeader);
    }

    private static void ReplaceService<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime)
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

    private static async Task AppendLogAsync(string path, string message)
    {
        try { await File.AppendAllTextAsync(path, $"[{DateTime.Now:O}] {message}{Environment.NewLine}"); } catch { }
    }
}
