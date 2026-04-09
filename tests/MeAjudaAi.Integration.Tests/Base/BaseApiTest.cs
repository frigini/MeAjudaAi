using System.Text.Json;
using MeAjudaAi.ApiService;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Integration.Tests.Mocks;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
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
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// Disable parallel execution to prevent race conditions when using shared database containers
[assembly: CollectionBehavior(DisableTestParallelization = true)]

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
    All = Users | Providers | Documents | ServiceCatalogs | Locations | SearchProviders | Communications
}

/// <summary>
/// Classe base unificada para testes de integração com suporte a autenticação baseada em instância.
/// </summary>
public abstract class BaseApiTest : IAsyncLifetime
{
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);
    
    private SimpleDatabaseFixture? _databaseFixture;
    private WireMockFixture? _wireMockFixture;
    private WebApplicationFactory<Program>? _factory;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _factory!.Services;
    protected ITestAuthenticationConfiguration AuthConfig { get; private set; } = null!;
    protected WireMockFixture WireMock => _wireMockFixture ?? throw new InvalidOperationException("WireMock not initialized");

    protected virtual TestModule RequiredModules => TestModule.All;
    protected virtual bool UseMockGeographicValidation => true;

    protected const string UserLocationHeader = "X-User-Location";
    protected const string ProvidersEndpoint = "/api/v1/providers";

    public async ValueTask InitializeAsync()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

        _wireMockFixture = new WireMockFixture();
        await _wireMockFixture.StartAsync();

        var wireMockUrl = _wireMockFixture.BaseUrl;
        Environment.SetEnvironmentVariable("Locations__ExternalApis__ViaCep__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__BrasilApi__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__OpenCep__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__Nominatim__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__IBGE__BaseUrl", $"{wireMockUrl}/api/v1/localidades");

        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();

#pragma warning disable CA2000 
        _factory = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                var apiServicePath = ResolveApiServicePath();
                if (!string.IsNullOrEmpty(apiServicePath)) builder.UseContentRoot(apiServicePath);

                builder.UseEnvironment("Testing");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Postgres:ConnectionString"] = _databaseFixture.ConnectionString,
                        ["ConnectionStrings:DefaultConnection"] = _databaseFixture.ConnectionString,
                        ["RabbitMQ:Enabled"] = "false",
                        ["Messaging:Enabled"] = "false",
                        ["Messaging:Provider"] = "Mock",
                        ["Keycloak:Enabled"] = "false",
                        ["FeatureManagement:GeographicRestriction"] = "true"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    RemoveDbContextRegistrations<UsersDbContext>(services);
                    RemoveDbContextRegistrations<ProvidersDbContext>(services);
                    RemoveDbContextRegistrations<DocumentsDbContext>(services);
                    RemoveDbContextRegistrations<ServiceCatalogsDbContext>(services);
                    RemoveDbContextRegistrations<LocationsDbContext>(services);
                    RemoveDbContextRegistrations<SearchProvidersDbContext>(services);
                    RemoveDbContextRegistrations<CommunicationsDbContext>(services);

                    ReconfigureCepProviderClients(services);

                    AddTestDbContext<UsersDbContext>(services, "users", "MeAjudaAi.Modules.Users.Infrastructure");
                    AddTestDbContext<ProvidersDbContext>(services, "providers", "MeAjudaAi.Modules.Providers.Infrastructure");
                    AddTestDbContext<DocumentsDbContext>(services, "documents", "MeAjudaAi.Modules.Documents.Infrastructure");
                    AddTestDbContext<ServiceCatalogsDbContext>(services, "service_catalogs", "MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                    AddTestDbContext<LocationsDbContext>(services, "locations", "MeAjudaAi.Modules.Locations.Infrastructure");
                    AddTestDbContext<SearchProvidersDbContext>(services, "search_providers", "MeAjudaAi.Modules.SearchProviders.Infrastructure");
                    AddTestDbContext<CommunicationsDbContext>(services, "communications", "MeAjudaAi.Modules.Communications.Infrastructure");

                    services.AddDocumentsTestServices(useAzurite: false);
                    services.AddSingleton<IBackgroundJobService, MockBackgroundJobService>();
                    services.AddHttpContextAccessor();

                    if (UseMockGeographicValidation)
                    {
                        var geoValidationDescriptors = services.Where(d => d.ServiceType == typeof(IGeographicValidationService)).ToList();
                        foreach (var descriptor in geoValidationDescriptors) services.Remove(descriptor);
                        services.AddScoped<IGeographicValidationService, MockGeographicValidationService>();
                    }

                    services.RemoveRealAuthentication();
                    services.AddInstanceTestAuthentication();

                    var claimsTransformationDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IClaimsTransformation));
                    if (claimsTransformationDescriptor != null) services.Remove(claimsTransformationDescriptor);
                });
            });

        Client = _factory.CreateClient();
        AuthConfig = _factory.Services.GetRequiredService<ITestAuthenticationConfiguration>();

        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<BaseApiTest>>();
        await ApplyRequiredModuleMigrationsAsync(scope.ServiceProvider, logger);
    }

    private void AddTestDbContext<TContext>(IServiceCollection services, string schema, string assembly) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(_databaseFixture!.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(assembly);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            });
            options.EnableSensitiveDataLogging();
            options.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
    }

    private async Task ApplyRequiredModuleMigrationsAsync(IServiceProvider serviceProvider, ILogger? logger)
    {
        var modules = RequiredModules;
        if (modules == TestModule.None) return;

        // Implied dependencies
        if (modules.HasFlag(TestModule.SearchProviders))
        {
            if (!modules.HasFlag(TestModule.Providers)) modules |= TestModule.Providers;
            if (!modules.HasFlag(TestModule.ServiceCatalogs)) modules |= TestModule.ServiceCatalogs;
            if (!modules.HasFlag(TestModule.Locations)) modules |= TestModule.Locations;
        }

        await MigrationLock.WaitAsync();
        try
        {
            // Use SearchProvidersDbContext as a proxy for cleanup if available, or any other
            DbContext cleanContext;
            if (modules.HasFlag(TestModule.SearchProviders)) cleanContext = serviceProvider.GetRequiredService<SearchProvidersDbContext>();
            else if (modules.HasFlag(TestModule.Users)) cleanContext = serviceProvider.GetRequiredService<UsersDbContext>();
            else cleanContext = serviceProvider.GetRequiredService<CommunicationsDbContext>();

            await EnsureCleanDatabaseAsync(cleanContext, logger);

            // Apply migrations in order
            if (modules.HasFlag(TestModule.Users)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<UsersDbContext>(), "Users", logger);
            if (modules.HasFlag(TestModule.ServiceCatalogs)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<ServiceCatalogsDbContext>(), "ServiceCatalogs", logger);
            if (modules.HasFlag(TestModule.Providers)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<ProvidersDbContext>(), "Providers", logger);
            if (modules.HasFlag(TestModule.Documents)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<DocumentsDbContext>(), "Documents", logger);
            if (modules.HasFlag(TestModule.Locations)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<LocationsDbContext>(), "Locations", logger);
            if (modules.HasFlag(TestModule.Communications)) await ApplyMigrationForContextAsync(serviceProvider.GetRequiredService<CommunicationsDbContext>(), "Communications", logger);
            
            if (modules.HasFlag(TestModule.SearchProviders))
            {
                var context = serviceProvider.GetRequiredService<SearchProvidersDbContext>();
                try 
                { 
                    await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS postgis;"); 
                } 
                catch (Exception ex)
                {
                    logger?.LogError(ex, "❌ Failed to create PostGIS extension.");
                }
                await ApplyMigrationForContextAsync(context, "SearchProviders", logger);
            }

            await SeedTestDataAsync(serviceProvider, logger);
            logger?.LogInformation("✅ Migrations and Seeding applied successfully");
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
                var providers = new List<SearchableProvider>
                {
                    MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider.Create(Guid.NewGuid(), "SP Close Provider", "sp-close", new GeoPoint(-23.5510, -46.6340), ESubscriptionTier.Gold),
                    MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider.Create(Guid.NewGuid(), "SP Nearby Provider", "sp-nearby", new GeoPoint(-23.5800, -46.6000), ESubscriptionTier.Standard),
                    MeAjudaAi.Modules.SearchProviders.Domain.Entities.SearchableProvider.Create(Guid.NewGuid(), "SP Far Provider", "sp-far", new GeoPoint(-23.4500, -46.5000), ESubscriptionTier.Free)
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
            try { await context.Database.EnsureDeletedAsync(); break; }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P03") { if (attempt == maxRetries) throw; await Task.Delay(TimeSpan.FromSeconds(attempt)); }
        }
    }

    private static async Task ApplyMigrationForContextAsync<TContext>(TContext context, string moduleName, ILogger? logger) where TContext : DbContext
    {
        Exception? lastException = null;
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try 
            { 
                await context.Database.MigrateAsync(); 
                return; 
            }
            catch (Exception ex)
            {
                lastException = ex;
                bool isTransient = ex is TimeoutException || 
                                  ex is DbUpdateException || 
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
        if (_databaseFixture != null) await _databaseFixture.DisposeAsync();
        if (_wireMockFixture != null) await _wireMockFixture.DisposeAsync();
    }

    private static void RemoveDbContextRegistrations<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (optionsDescriptor != null) services.Remove(optionsDescriptor);
        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor != null) services.Remove(contextDescriptor);
    }

    private void ReconfigureCepProviderClients(IServiceCollection services)
    {
        services.AddHttpClient<ViaCepClient>(c => c.BaseAddress = new Uri(_wireMockFixture!.BaseUrl));
        services.AddHttpClient<BrasilApiCepClient>(c => c.BaseAddress = new Uri(_wireMockFixture!.BaseUrl));
        services.AddHttpClient<OpenCepClient>(c => c.BaseAddress = new Uri(_wireMockFixture!.BaseUrl));
        services.AddHttpClient<IbgeClient>(c => c.BaseAddress = new Uri(_wireMockFixture!.BaseUrl + "/api/v1/localidades/"));
        services.AddHttpClient<NominatimClient>(c => c.BaseAddress = new Uri(_wireMockFixture!.BaseUrl));
    }

    protected async Task<T?> ReadJsonAsync<T>(HttpContent content)
    {
        var jsonString = await content.ReadAsStringAsync();
        try 
        {
            var json = JsonSerializer.Deserialize<JsonElement>(jsonString, SerializationDefaults.Api);
            if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("isSuccess", out var s) && s.ValueKind == JsonValueKind.True && json.TryGetProperty("value", out var v))
                return JsonSerializer.Deserialize<T>(v.GetRawText(), SerializationDefaults.Api);
            return JsonSerializer.Deserialize<T>(jsonString, SerializationDefaults.Api);
        }
        catch { return JsonSerializer.Deserialize<T>(jsonString, SerializationDefaults.Api); }
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
