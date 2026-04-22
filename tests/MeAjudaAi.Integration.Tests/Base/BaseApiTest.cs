using System.Text.Json;
using MeAjudaAi.ApiService;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Integration.Tests.Mocks;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Modules.Documents.Tests;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Runtime.CompilerServices;

// Enable parallel execution by isolating databases per test class
// [assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Módulos disponíveis para testes de integração
/// </summary>
[Flags]
public enum TestModule
{
    None = 0,
    Users = 1 << 0,
    Providers = 1 << 1,
    Documents = 1 << 2,
    ServiceCatalogs = 1 << 3,
    Locations = 1 << 4,
    SearchProviders = 1 << 5,
    Communications = 1 << 6,
    Payments = 1 << 7,
    Bookings = 1 << 8,
    All = Users | Providers | Documents | ServiceCatalogs | Locations | SearchProviders | Communications | Payments | Bookings
    }

/// <summary>
/// Classe base unificada para testes de integração com suporte a autenticação baseada em instância.
/// </summary>
public abstract class BaseApiTest : IAsyncLifetime
{
    [ModuleInitializer]
    public static void InitializeNpgsql()
    {
        // ⚠️ CRÍTICO: Configura Npgsql ANTES de qualquer operação de banco no processo de teste
        // Correção para compatibilidade DateTime UTC com PostgreSQL timestamp
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    private static readonly SemaphoreSlim MigrationLock = new(1, 1);
    
    private SimpleDatabaseFixture? _databaseFixture;
    private WireMockFixture? _wireMockFixture;
    protected WebApplicationFactory<Program>? _factory;
    private string? _databaseName;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _factory!.Services;
    protected ITestAuthenticationConfiguration AuthConfig { get; private set; } = null!;
    protected WireMockFixture WireMock => _wireMockFixture ?? throw new InvalidOperationException("WireMock not initialized");
    protected string DatabaseName => _databaseName ??= $"test_{GetType().Name.ToLowerInvariant()}_{Guid.NewGuid().ToString("n")[..8]}";

    protected virtual TestModule RequiredModules => TestModule.All;
    protected virtual bool UseMockGeographicValidation => true;

    protected const string UserLocationHeader = "X-User-Location";
    protected const string ProvidersEndpoint = "/api/v1/providers";
    public static readonly Guid TestServiceId = Guid.Parse("d3b07384-d9a6-4475-bd61-1c3906d4e135");

    public async ValueTask InitializeAsync()
    {

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

        _wireMockFixture = new WireMockFixture();
        await _wireMockFixture.StartAsync();

        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();

        // Criar banco de dados isolado para esta classe de teste
        await _databaseFixture.CreateDatabaseAsync(DatabaseName);
        var connectionString = _databaseFixture.GetConnectionString(DatabaseName);

    #pragma warning disable CA2000 
        _factory = new WebApplicationFactory<Program>()
    #pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                var apiServicePath = ResolveApiServicePath();
                if (!string.IsNullOrEmpty(apiServicePath)) builder.UseContentRoot(apiServicePath);

                builder.UseEnvironment("Testing");
                builder.UseSetting("https_port", "443");

                var wireMockUrl = _wireMockFixture!.BaseUrl;

                // Configurar URLs do WireMock nos provedores de CEP específicos para esta instância
                builder.UseSetting("Locations:ExternalApis:ViaCep:BaseUrl", wireMockUrl);
                builder.UseSetting("Locations:ExternalApis:BrasilApi:BaseUrl", wireMockUrl);
                builder.UseSetting("Locations:ExternalApis:OpenCep:BaseUrl", wireMockUrl);
                builder.UseSetting("Locations:ExternalApis:Nominatim:BaseUrl", wireMockUrl);
                builder.UseSetting("Locations:ExternalApis:IBGE:BaseUrl", $"{wireMockUrl}/api/v1/localidades");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Information",
                        ["Postgres:ConnectionString"] = connectionString,
                        ["ConnectionStrings:DefaultConnection"] = connectionString,
                        ["RabbitMQ:Enabled"] = "false",
                        ["Messaging:Enabled"] = "false",
                        ["Messaging:Provider"] = "Mock",
                        ["Keycloak:Enabled"] = "false",
                        ["FeatureManagement:GeographicRestriction"] = "true",
                        ["Locations:ExternalApis:ViaCep:BaseUrl"] = wireMockUrl,
                        ["Locations:ExternalApis:BrasilApi:BaseUrl"] = wireMockUrl,
                        ["Locations:ExternalApis:OpenCep:BaseUrl"] = wireMockUrl,
                        ["Locations:ExternalApis:Nominatim:BaseUrl"] = wireMockUrl,
                        ["Locations:ExternalApis:IBGE:BaseUrl"] = $"{wireMockUrl}/api/v1/localidades",
                        ["GeographicRestriction:AllowedCities:0"] = "Muriaé",
                        ["GeographicRestriction:AllowedCities:1"] = "Itaperuna",
                        ["GeographicRestriction:AllowedCities:2"] = "Linhares",
                        ["GeographicRestriction:AllowedStates:0"] = "MG",
                        ["GeographicRestriction:AllowedStates:1"] = "RJ",
                        ["GeographicRestriction:AllowedStates:2"] = "ES",
                        ["Cache:Enabled"] = "false",
                        ["RateLimit:Enabled"] = "false",
                        ["AdvancedRateLimit:General:Enabled"] = "false"
                    });
                });
                builder.ConfigureServices(services =>
                {
                    // Forçar o uso de cache em memória para IDistributedCache
                    services.AddDistributedMemoryCache();

                    RemoveDbContextRegistrations<UsersDbContext>(services);
                    RemoveDbContextRegistrations<ProvidersDbContext>(services);
                    RemoveDbContextRegistrations<DocumentsDbContext>(services);
                    RemoveDbContextRegistrations<ServiceCatalogsDbContext>(services);
                    RemoveDbContextRegistrations<LocationsDbContext>(services);
                    RemoveDbContextRegistrations<SearchProvidersDbContext>(services);
                    RemoveDbContextRegistrations<CommunicationsDbContext>(services);
                    RemoveDbContextRegistrations<PaymentsDbContext>(services);
                    RemoveDbContextRegistrations<BookingsDbContext>(services);

                    AddTestDbContext<UsersDbContext>(services, "users", "MeAjudaAi.Modules.Users.Infrastructure");
                    AddTestDbContext<ProvidersDbContext>(services, "providers", "MeAjudaAi.Modules.Providers.Infrastructure");
                    AddTestDbContext<DocumentsDbContext>(services, "documents", "MeAjudaAi.Modules.Documents.Infrastructure");
                    AddTestDbContext<ServiceCatalogsDbContext>(services, "service_catalogs", "MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                    AddTestDbContext<LocationsDbContext>(services, "locations", "MeAjudaAi.Modules.Locations.Infrastructure");
                    AddTestDbContext<SearchProvidersDbContext>(services, "search_providers", "MeAjudaAi.Modules.SearchProviders.Infrastructure");
                    AddTestDbContext<CommunicationsDbContext>(services, "communications", "MeAjudaAi.Modules.Communications.Infrastructure");
                    AddTestDbContext<PaymentsDbContext>(services, "payments", "MeAjudaAi.Modules.Payments.Infrastructure");
                    AddTestDbContext<BookingsDbContext>(services, "bookings", "MeAjudaAi.Modules.Bookings.Infrastructure");

                    services.AddDocumentsTestServices(useAzurite: false);
                    services.AddSingleton<IBackgroundJobService, MockBackgroundJobService>();
                    
                    // Always mock IPaymentGateway to ensure no real external calls are made
                    var paymentGatewayDescriptors = services.Where(d => d.ServiceType == typeof(IPaymentGateway)).ToList();
                    foreach (var descriptor in paymentGatewayDescriptors) services.Remove(descriptor);
                    services.AddScoped<IPaymentGateway, MockPaymentGateway>();

                    // Register dummy Stripe client to satisfy DI validation
                    services.AddSingleton<Stripe.IStripeClient>(new Stripe.StripeClient("sk_test_dummy"));
                    
                    services.AddHttpContextAccessor();

                    if (UseMockGeographicValidation)
                    {
                        var geoValidationDescriptors = services.Where(d => d.ServiceType == typeof(IGeographicValidationService)).ToList();
                        foreach (var descriptor in geoValidationDescriptors) services.Remove(descriptor);
                        services.AddScoped<IGeographicValidationService, MockGeographicValidationService>();
                    }

                    services.RemoveRealAuthentication();
                    services.AddInstanceTestAuthentication();
                });
            });

        var options = new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
            BaseAddress = new Uri("https://localhost")
        };
        Client = _factory.CreateClient(options);
        AuthConfig = _factory.Services.GetRequiredService<ITestAuthenticationConfiguration>();

        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<BaseApiTest>>();
        await ApplyRequiredModuleMigrationsAsync(scope.ServiceProvider, logger);
    }

    private void AddTestDbContext<TContext>(IServiceCollection services, string schema, string assembly) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            var connectionString = _databaseFixture!.GetConnectionString(DatabaseName);
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseNetTopologySuite();
                npgsqlOptions.MigrationsAssembly(assembly);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            });
            
            options.UseSnakeCaseNamingConvention();
            
            // Suprime aviso de mudanças pendentes no modelo durante testes de integração.
            // Útil para ignorar drifts menores de convenção de nomes (ex: PK_ casing) sem forçar novas migrations em dev.
            options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

            if (Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_LOGGING") == "true")
            {
                options.EnableSensitiveDataLogging();
            }
        });
    }

    private async Task ApplyRequiredModuleMigrationsAsync(IServiceProvider serviceProvider, ILogger? logger)
    {
        var modules = RequiredModules;
        if (modules == TestModule.None) return;

        // Dependências implícitas
        if (modules.HasFlag(TestModule.SearchProviders))
        {
            if (!modules.HasFlag(TestModule.Providers)) modules |= TestModule.Providers;
            if (!modules.HasFlag(TestModule.ServiceCatalogs)) modules |= TestModule.ServiceCatalogs;
            if (!modules.HasFlag(TestModule.Locations)) modules |= TestModule.Locations;
        }

        if (modules.HasFlag(TestModule.Providers))
        {
            if (!modules.HasFlag(TestModule.ServiceCatalogs)) modules |= TestModule.ServiceCatalogs;
        }

        if (modules.HasFlag(TestModule.Bookings))
        {
            if (!modules.HasFlag(TestModule.Users)) modules |= TestModule.Users;
            if (!modules.HasFlag(TestModule.ServiceCatalogs)) modules |= TestModule.ServiceCatalogs;
            if (!modules.HasFlag(TestModule.Providers)) modules |= TestModule.Providers;
        }

        // Lock para evitar que múltiplas migrações ocorram simultaneamente no MESMO banco, 
        // mas como os bancos agora são isolados, o lock serve apenas como precaução 
        // caso algo tente acessar o banco master simultaneamente.
        await MigrationLock.WaitAsync();
        try
        {
            // Apply migrations in production priority order: Users -> ServiceCatalogs -> Locations -> Documents -> Providers -> Communications
            if (modules.HasFlag(TestModule.Users)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<UsersDbContext>(), "Users", logger);
            if (modules.HasFlag(TestModule.ServiceCatalogs)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<ServiceCatalogsDbContext>(), "ServiceCatalogs", logger);
            if (modules.HasFlag(TestModule.Locations)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<LocationsDbContext>(), "Locations", logger);
            if (modules.HasFlag(TestModule.Documents)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<DocumentsDbContext>(), "Documents", logger);
            if (modules.HasFlag(TestModule.Providers)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<ProvidersDbContext>(), "Providers", logger);
            if (modules.HasFlag(TestModule.Communications)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<CommunicationsDbContext>(), "Communications", logger);
            if (modules.HasFlag(TestModule.Payments)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<PaymentsDbContext>(), "Payments", logger);
            if (modules.HasFlag(TestModule.Bookings)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<BookingsDbContext>(), "Bookings", logger);
            
            if (modules.HasFlag(TestModule.SearchProviders))
            {
                var context = serviceProvider.GetRequiredService<SearchProvidersDbContext>();
                await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS postgis;"); 
                await ApplyMigrationForContextAsync(context, "SearchProviders", logger);
            }

            await SeedTestDataAsync(serviceProvider, logger);
            logger?.LogInformation("✅ Migrations and Seeding applied successfully to {DbName}", DatabaseName);
        }
        finally { MigrationLock.Release(); }
    }

    private async Task SeedTestDataAsync(IServiceProvider serviceProvider, ILogger? logger)
    {
        var modules = RequiredModules;

        if (modules.HasFlag(TestModule.Locations) || modules.HasFlag(TestModule.SearchProviders))
        {
            var locationsContext = serviceProvider.GetRequiredService<LocationsDbContext>();
            var testCities = new[]
            {
                new { IbgeCode = 3143906, CityName = "Muriaé", State = "MG" },
                new { IbgeCode = 3302504, CityName = "Itaperuna", State = "RJ" },
                new { IbgeCode = 3203205, CityName = "Linhares", State = "ES" }
            };

            foreach (var city in testCities)
            {
                if (!await locationsContext.AllowedCities.AnyAsync(c => c.CityName == city.CityName && c.StateSigla == city.State))
                {
                    locationsContext.AllowedCities.Add(new MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity(city.CityName, city.State, "system", city.IbgeCode));
                }
            }
            await locationsContext.SaveChangesAsync();
        }

        if (modules.HasFlag(TestModule.SearchProviders))
        {
            var searchContext = serviceProvider.GetRequiredService<SearchProvidersDbContext>();
            if (!await searchContext.SearchableProviders.AnyAsync())
            {
                var closeProvider = MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider.Create(Guid.NewGuid(), "SP Close Provider", "sp-close", new GeoPoint(-23.5501, -46.6330), ESubscriptionTier.Gold);
                closeProvider.UpdateServices(new[] { TestServiceId });

                var providers = new List<SearchableProvider>
                {
                    closeProvider, // ~50m from center (-23.5505, -46.6333) with TestServiceId
                    MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider.Create(Guid.NewGuid(), "SP Nearby Provider", "sp-nearby", new GeoPoint(-23.5550, -46.6400), ESubscriptionTier.Standard), // ~850m from center
                    MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider.Create(Guid.NewGuid(), "SP Far Provider", "sp-far", new GeoPoint(-23.6000, -46.7000), ESubscriptionTier.Free) // ~8km from center
                };
                searchContext.SearchableProviders.AddRange(providers);
                await searchContext.SaveChangesAsync();
            }
        }
    }

    private static async Task EnsureCleanDatabaseAsync(DbContext context, ILogger? logger)
    {
        const int maxRetries = 10;
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try 
            { 
                // Clear all connection pools to prevent "database in use" errors
                if (attempt == 1)
                {
                    Npgsql.NpgsqlConnection.ClearAllPools();
                }

                await context.Database.EnsureDeletedAsync(); 
                break; 
            }
            catch (Exception ex) when (IsTransientException(ex))
            { 
                if (attempt == maxRetries) throw; 
                
                var sqlState = (ex as Npgsql.PostgresException)?.SqlState ?? 
                              (ex.InnerException as Npgsql.PostgresException)?.SqlState ?? "Unknown";

                logger?.LogWarning("⚠️ Database cleanup attempt {Attempt}/{Max} due to transient error {SqlState}. Message: {Message}", 
                    attempt, maxRetries, sqlState, ex.Message);
                
                await Task.Delay(1000 * attempt);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ Deterministic error during database cleanup on attempt {Attempt}", attempt);
                throw;
            }
        }
    }

    private static bool IsTransientException(Exception ex)
    {
        // Direct PostgresException
        if (ex is Npgsql.PostgresException pgEx && (pgEx.SqlState == "57P03" || pgEx.SqlState == "57P01" || pgEx.SqlState == "55006" || pgEx.SqlState == "53300"))
            return true;

        // EF Core often wraps transient errors into InvalidOperationException
        if (ex is InvalidOperationException && ex.Message.Contains("transient failure") && ex.InnerException != null)
            return IsTransientException(ex.InnerException);

        // DbUpdateException often wraps Npgsql exceptions
        if (ex is Microsoft.EntityFrameworkCore.DbUpdateException && ex.InnerException != null)
            return IsTransientException(ex.InnerException);

        return false;
    }

    private static async Task ApplyMigrationForContextAsync<TContext>(TContext context, string moduleName, ILogger? logger) where TContext : DbContext
    {
        Exception? lastException = null;
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try 
            { 
                context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));
                await context.Database.MigrateAsync(); 
                return; 
            }
            catch (Exception ex)
            {
                lastException = ex;
                bool isTransient = ex is TimeoutException ||
                                  (ex is Npgsql.PostgresException pgEx && (pgEx.SqlState == "57P01" || pgEx.SqlState == "53300" || pgEx.SqlState == "08006"));

                if (!isTransient || attempt == 3) break;
                await Task.Delay(1000 * attempt);
            }
        }
        throw new InvalidOperationException($"Failed to apply {moduleName} migrations.", lastException);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null && _databaseName != null) await _databaseFixture.DropDatabaseAsync(_databaseName);
        if (_wireMockFixture != null) await _wireMockFixture.DisposeAsync();
    }

    private static void RemoveDbContextRegistrations<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (optionsDescriptor != null) services.Remove(optionsDescriptor);
        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor != null) services.Remove(contextDescriptor);
    }

    protected async Task<T?> ReadJsonAsync<T>(HttpContent content)
    {
        var jsonString = await content.ReadAsStringAsync();
        try 
        {
            var json = JsonSerializer.Deserialize<JsonElement>(jsonString, SerializationDefaults.Api);
            
            // Só tentamos desembrulhar se:
            // 1. O JSON for um objeto com a estrutura do Result { items: ..., isSuccess: true, value: ... }
            // 2. O tipo solicitado T NÃO for do tipo Result<T> ou Response<T> (para evitar erro de dupla desserialização)
            
            var type = typeof(T);
            bool isResultType = false;

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                isResultType = genericTypeDefinition.Name == "Result`1" || 
                               genericTypeDefinition.Name == "Response`1";
            }
            else
            {
                // Tratar tipos de wrapper de resposta não genéricos (ex: UploadDocumentResponse)
                isResultType = type.Name.EndsWith("Response") || 
                               (type.Namespace?.Contains("Contracts.Functional") ?? false);
            }

            if (!isResultType && json.ValueKind == JsonValueKind.Object && json.TryGetProperty("isSuccess", out var s) && s.ValueKind == JsonValueKind.True && json.TryGetProperty("value", out var v))
                return JsonSerializer.Deserialize<T>(v.GetRawText(), SerializationDefaults.Api);
            
            return JsonSerializer.Deserialize<T>(jsonString, SerializationDefaults.Api);
        }
        catch (JsonException ex)
        {
            var preview = BuildSafeResponsePreview(jsonString);
            throw new InvalidOperationException($"Failed to deserialize JSON response to type {typeof(T).Name}. Content Preview: {preview}", ex);
        }
        catch (Exception ex)
        {
            var preview = BuildSafeResponsePreview(jsonString);
            throw new InvalidOperationException($"An unexpected error occurred while processing the API response. Preview: {preview}", ex);
        }
    }

    private static string BuildSafeResponsePreview(string content, int maxLength = 1000)
    {
        if (string.IsNullOrEmpty(content)) return "[Empty Content]";
        return content.Length <= maxLength ? content : content[..maxLength] + "... [TRUNCATED]";
    }

    protected static JsonElement GetResponseData(JsonElement response)
    {
        if (response.ValueKind == JsonValueKind.Array) return response;
        if (response.ValueKind == JsonValueKind.Object)
        {
            if (response.TryGetProperty("value", out var v)) return v;
            if (response.TryGetProperty("data", out var d)) return d;
        }
        return response;
    }

    private static string? ResolveApiServicePath()
    {
        var envPath = Environment.GetEnvironmentVariable("MEAJUDAAI_API_SERVICE_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath)) return envPath;
        var assemblyDir = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(assemblyDir))
        {
            var candidatePath = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "..", "src", "Bootstrapper", "MeAjudaAi.ApiService"));
            if (Directory.Exists(candidatePath)) return candidatePath;
        }
        return null;
    }
}
