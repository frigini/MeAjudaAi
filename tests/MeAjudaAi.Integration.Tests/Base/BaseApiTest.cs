using System.Reflection;
using System.Text.Json;
using MeAjudaAi.ApiService;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Integration.Tests.Mocks;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks;
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
    All = Users | Providers | Documents | ServiceCatalogs | Locations | SearchProviders
}

/// <summary>
/// Classe base unificada para testes de integração com suporte a autenticação baseada em instância.
/// Elimina condições de corrida e instabilidade causadas por estado estático.
/// Cria containers individuais para máxima compatibilidade com CI.
/// Aplica migrations apenas dos módulos necessários (especificados via RequiredModules).
/// </summary>
public abstract class BaseApiTest : IAsyncLifetime
{
    // Semáforo estático para sincronizar aplicação de migrations entre testes paralelos
    private static readonly SemaphoreSlim MigrationLock = new(1, 1);
    
    private SimpleDatabaseFixture? _databaseFixture;
    private WireMockFixture? _wireMockFixture;
    private WebApplicationFactory<Program>? _factory;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _factory!.Services;
    protected ITestAuthenticationConfiguration AuthConfig { get; private set; } = null!;
    protected WireMockFixture WireMock => _wireMockFixture ?? throw new InvalidOperationException("WireMock not initialized");

    /// <summary>
    /// Especifica quais módulos este teste precisa (migrations serão aplicadas apenas para estes).
    /// Override em classes derivadas para otimizar tempo de inicialização.
    /// Default: All (comportamento legado para compatibilidade).
    /// </summary>
    protected virtual TestModule RequiredModules => TestModule.All;

    /// <summary>
    /// HTTP header name for user location (format: "City|State")
    /// </summary>
    protected const string UserLocationHeader = "X-User-Location";

    /// <summary>
    /// API endpoint for providers listing
    /// </summary>
    protected const string ProvidersEndpoint = "/api/v1/providers";

    /// <summary>
    /// Controls whether to use mock geographic validation service.
    /// Set to false in IBGE-focused tests to use real service with WireMock.
    /// </summary>
    protected virtual bool UseMockGeographicValidation => true;

    public async ValueTask InitializeAsync()
    {
        // Define variáveis de ambiente para testes
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

        // Inicializa WireMock antes da aplicação para que as URLs mockadas estejam disponíveis
        _wireMockFixture = new WireMockFixture();
        await _wireMockFixture.StartAsync();

        // Configure environment variables with dynamic WireMock URLs
        var wireMockUrl = _wireMockFixture.BaseUrl;
        Environment.SetEnvironmentVariable("Locations__ExternalApis__ViaCep__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__BrasilApi__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__OpenCep__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__Nominatim__BaseUrl", wireMockUrl);
        Environment.SetEnvironmentVariable("Locations__ExternalApis__IBGE__BaseUrl", $"{wireMockUrl}/api/v1/localidades");

        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();

#pragma warning disable CA2000 // Dispose é gerenciado por IAsyncLifetime.DisposeAsync
        _factory = new WebApplicationFactory<Program>()
#pragma warning restore CA2000
            .WithWebHostBuilder(builder =>
            {
                // Resolve ApiService content root using robust path resolution
                var apiServicePath = ResolveApiServicePath();
                if (!string.IsNullOrEmpty(apiServicePath))
                {
                    builder.UseContentRoot(apiServicePath);
                }
                else
                {
                    Console.Error.WriteLine("WARNING: Could not resolve ApiService content root path. Configuration files may not load correctly.");
                }

                builder.UseEnvironment("Testing");

                // Inject test configuration directly to ensure consistent behavior across environments
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft.AspNetCore"] = "Warning",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Warning",
                        ["RateLimit:DefaultRequestsPerMinute"] = "10000",
                        ["RateLimit:AuthRequestsPerMinute"] = "10000",
                        ["RateLimit:SearchRequestsPerMinute"] = "10000",
                        ["RateLimit:WindowInSeconds"] = "60",
                        ["Caching:Enabled"] = "false",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Messaging:Enabled"] = "false",
                        ["Messaging:Provider"] = "Mock",
                        ["Keycloak:Enabled"] = "false",
                        ["Keycloak:ClientSecret"] = "test-secret",
                        ["Keycloak:AdminUsername"] = "test-admin",
                        ["Keycloak:AdminPassword"] = "test-password",
                        ["FeatureManagement:GeographicRestriction"] = "true",
                        ["FeatureManagement:PushNotifications"] = "false",
                        ["FeatureManagement:StripePayments"] = "false",
                        ["FeatureManagement:MaintenanceMode"] = "false",
                        // Geographic restriction: Cities with states in "City|State" format
                        // This ensures proper validation when both city and state headers are provided
                        ["GeographicRestriction:AllowedStates:0"] = "MG",
                        ["GeographicRestriction:AllowedStates:1"] = "ES",
                        ["GeographicRestriction:AllowedStates:2"] = "RJ",
                        ["GeographicRestriction:AllowedCities:0"] = "Muriaé|MG",
                        ["GeographicRestriction:AllowedCities:1"] = "Itaperuna|RJ",
                        ["GeographicRestriction:AllowedCities:2"] = "Linhares|ES",
                        ["GeographicRestriction:BlockedMessage"] = "Serviço indisponível na sua região. Disponível apenas em: {allowedRegions}"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    // Substitui banco de dados por container de teste - Remove todos os serviços relacionados ao DbContext
                    RemoveDbContextRegistrations<UsersDbContext>(services);
                    RemoveDbContextRegistrations<ProvidersDbContext>(services);
                    RemoveDbContextRegistrations<DocumentsDbContext>(services);
                    RemoveDbContextRegistrations<ServiceCatalogsDbContext>(services);
                    RemoveDbContextRegistrations<LocationsDbContext>(services);
                    RemoveDbContextRegistrations<SearchProvidersDbContext>(services);

                    // Reconfigure CEP provider HttpClients to use WireMock
                    ReconfigureCepProviderClients(services);

                    // Adiciona contextos de banco de dados para testes
                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    services.AddDbContext<ProvidersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    services.AddDbContext<MeAjudaAi.Modules.Documents.Infrastructure.Persistence.DocumentsDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Documents.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    services.AddDbContext<ServiceCatalogsDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "service_catalogs");
                        });
                        options.UseSnakeCaseNamingConvention();
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    services.AddDbContext<LocationsDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Locations.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "locations");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    services.AddDbContext<SearchProvidersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.SearchProviders.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search_providers");
                        });
                        options.EnableSensitiveDataLogging();
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    // Adiciona mocks de serviços para testes
                    // TODO: Investigar problema de SAS token do Azurite e migrar de Mock para emulador Azurite
                    // Atualmente usando Mock porque Azurite retorna erros 500 em testes de upload (problema CanGenerateSasUri).
                    // Ver issue de rastreamento: https://github.com/frigini/MeAjudaAi/issues/1
                    // Passos de investigação: verificar logs Azurite, testar criação de container manualmente, validar compatibilidade Azurite 3.33.0 com SAS tokens
                    services.AddDocumentsTestServices(useAzurite: false);

                    // Mock do BackgroundJobService para evitar execução de jobs em testes
                    services.AddSingleton<IBackgroundJobService, MockBackgroundJobService>();

                    // Adiciona HttpContextAccessor necessário para alguns handlers
                    services.AddHttpContextAccessor();

                    // Conditionally replace geographic validation with mock
                    // IBGE-focused tests can override UseMockGeographicValidation to use real service with WireMock
                    if (UseMockGeographicValidation)
                    {
                        // Remove ALL instances of IGeographicValidationService
                        var geoValidationDescriptors = services
                            .Where(d => d.ServiceType == typeof(IGeographicValidationService))
                            .ToList();
                        
                        foreach (var descriptor in geoValidationDescriptors)
                        {
                            services.Remove(descriptor);
                        }

                        // Registra mock com cidades piloto padrão (Scoped para isolamento entre testes)
                        services.AddScoped<IGeographicValidationService, MockGeographicValidationService>();
                    }

                    // Adiciona autenticação de teste baseada em instância para evitar estado estático
                    services.RemoveRealAuthentication();
                    services.AddInstanceTestAuthentication();

                    // Remove ClaimsTransformation que causa travamentos nos testes
                    var claimsTransformationDescriptor = services.FirstOrDefault(d =>
                        d.ServiceType == typeof(IClaimsTransformation));
                    if (claimsTransformationDescriptor != null)
                        services.Remove(claimsTransformationDescriptor);
                });

                // Habilita logging detalhado para debug
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Debug);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Debug);
                    logging.AddFilter("MeAjudaAi", LogLevel.Debug);
                });
            });

        Client = _factory.CreateClient();

        // Obtém a configuração de autenticação da instância do container DI
        AuthConfig = _factory.Services.GetRequiredService<ITestAuthenticationConfiguration>();

        // Aplica migrações apenas dos módulos necessários (otimização de performance)
        using var scope = _factory.Services.CreateScope();
        var logger = scope.ServiceProvider.GetService<ILogger<BaseApiTest>>();

        await ApplyRequiredModuleMigrationsAsync(scope.ServiceProvider, logger);
    }

    private static async Task SeedTestDataAsync(LocationsDbContext locationsContext, ILogger? logger)
    {
        // Seed allowed cities for GeographicRestriction tests
        // These match the cities configured in test configuration (lines 122-124)
        var testCities = new[]
        {
            new { IbgeCode = 3143906, CityName = "Muriaé", State = "MG" },
            new { IbgeCode = 3302504, CityName = "Itaperuna", State = "RJ" },
            new { IbgeCode = 3203205, CityName = "Linhares", State = "ES" }
        };

        var citiesToAdd = new List<MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity>();

        foreach (var city in testCities)
        {
            // Check if city already exists to avoid duplicate key errors
            var exists = await locationsContext.AllowedCities
                .AnyAsync(c => c.CityName == city.CityName && c.StateSigla == city.State);
            
            if (!exists)
            {
                // Use EF Core entity instead of raw SQL to avoid case sensitivity issues
                var allowedCity = new MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity(
                    city.CityName,
                    city.State,
                    "system",
                    city.IbgeCode);
                
                citiesToAdd.Add(allowedCity);
            }
            else
            {
                logger?.LogDebug("City {City}/{State} already exists, skipping", city.CityName, city.State);
            }
        }

        if (citiesToAdd.Count > 0)
        {
            locationsContext.AllowedCities.AddRange(citiesToAdd);
            await locationsContext.SaveChangesAsync();
            logger?.LogInformation("✅ Seeded {Count} test cities", citiesToAdd.Count);
        }

        var totalCount = await locationsContext.AllowedCities.CountAsync();
        logger?.LogInformation("Total cities in database: {Count}", totalCount);
    }

    /// <summary>
    /// Aplica migrations apenas dos módulos especificados em RequiredModules.
    /// Otimiza tempo de inicialização e evita race conditions ao aplicar apenas o necessário.
    /// </summary>
    private async Task ApplyRequiredModuleMigrationsAsync(IServiceProvider serviceProvider, ILogger? logger)
    {
        var modules = RequiredModules;
        
        // Se nenhum módulo especificado, retorna sem fazer nada
        if (modules == TestModule.None)
        {
            logger?.LogInformation("ℹ️ No modules required - skipping migrations");
            return;
        }

        // Implicit dependencies: satisfying cross-module database requirements
        // 1. Providers module depends on ServiceCatalogs during migration (AddProviderProfileEnhancements)
        if (modules.HasFlag(TestModule.Providers) && !modules.HasFlag(TestModule.ServiceCatalogs))
        {
            logger?.LogInformation("🔄 Adding implicit ServiceCatalogs dependency for Providers module");
            modules |= TestModule.ServiceCatalogs;
        }

        // 2. SearchProviders depends on Providers and ServiceCatalogs for its read model
        if (modules.HasFlag(TestModule.SearchProviders))
        {
            if (!modules.HasFlag(TestModule.Providers))
            {
                logger?.LogInformation("🔄 Adding implicit Providers dependency for SearchProviders module");
                modules |= TestModule.Providers;
            }
            if (!modules.HasFlag(TestModule.ServiceCatalogs))
            {
                logger?.LogInformation("🔄 Adding implicit ServiceCatalogs dependency for SearchProviders module");
                modules |= TestModule.ServiceCatalogs;
            }
        }

        logger?.LogInformation("🔄 Applying migrations for modules: {Modules}", modules);

        // Usa semáforo para garantir que apenas um teste aplique migrations por vez
        // Evita race conditions e erro "57P01: terminating connection due to administrator command"
        await MigrationLock.WaitAsync();
        try
        {
            // Garante estado limpo do banco de dados apenas uma vez
            DbContext anyContext;
            if (modules.HasFlag(TestModule.Users))
                anyContext = serviceProvider.GetRequiredService<UsersDbContext>();
            else if (modules.HasFlag(TestModule.Documents))
                anyContext = serviceProvider.GetRequiredService<DocumentsDbContext>();
            else if (modules.HasFlag(TestModule.Providers))
                anyContext = serviceProvider.GetRequiredService<ProvidersDbContext>();
            else if (modules.HasFlag(TestModule.ServiceCatalogs))
                anyContext = serviceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            else if (modules.HasFlag(TestModule.Locations))
                anyContext = serviceProvider.GetRequiredService<LocationsDbContext>();
            else
                anyContext = serviceProvider.GetRequiredService<SearchProvidersDbContext>();

            await EnsureCleanDatabaseAsync(anyContext, logger);

            // Aplica migrations dos módulos necessários
            if (modules.HasFlag(TestModule.Users))
            {
                var context = serviceProvider.GetRequiredService<UsersDbContext>();
                await ApplyMigrationForContextAsync(context, "Users", logger, "UsersDbContext");
                await context.Database.CloseConnectionAsync();
            }

            if (modules.HasFlag(TestModule.ServiceCatalogs))
            {
                var context = serviceProvider.GetRequiredService<ServiceCatalogsDbContext>();
                await ApplyMigrationForContextAsync(context, "ServiceCatalogs", logger, "ServiceCatalogsDbContext");
                await context.Database.CloseConnectionAsync();
            }

            if (modules.HasFlag(TestModule.Providers))
            {
                var context = serviceProvider.GetRequiredService<ProvidersDbContext>();
                await ApplyMigrationForContextAsync(context, "Providers", logger, "ProvidersDbContext");
                await context.Database.CloseConnectionAsync();
            }

            if (modules.HasFlag(TestModule.Documents))
            {
                var context = serviceProvider.GetRequiredService<DocumentsDbContext>();
                await ApplyMigrationForContextAsync(context, "Documents", logger, "DocumentsDbContext");
                await context.Database.CloseConnectionAsync();
            }

            if (modules.HasFlag(TestModule.Locations))
            {
                var context = serviceProvider.GetRequiredService<LocationsDbContext>();
                await ApplyMigrationForContextAsync(context, "Locations", logger, "LocationsDbContext");
                
                // Seed test data for allowed cities (required for GeographicRestriction tests)
                // Must be called BEFORE CloseConnectionAsync to use the already-open connection
                await SeedTestDataAsync(context, logger);
                
                await context.Database.CloseConnectionAsync();
            }

            if (modules.HasFlag(TestModule.SearchProviders))
            {
                var context = serviceProvider.GetRequiredService<SearchProvidersDbContext>();
                
                // Ensure PostGIS extension exists (required for geometry types)
                // This is necessary because EnsureDeletedAsync drops the database and extension
                // And we need it before SearchProviders migrations if they use geometry types
                try 
                {
                   await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS postgis;");
                } 
                catch (Exception ex)
                {
                   logger?.LogWarning(ex, "⚠️ Failed to explicitly create PostGIS extension. Migrations might fail if not included.");
                }

                await ApplyMigrationForContextAsync(context, "SearchProviders", logger, "SearchProvidersDbContext");
                await context.Database.CloseConnectionAsync();
            }

            logger?.LogInformation("✅ Migrations applied for required modules");
        }
        finally
        {
            MigrationLock.Release();
        }
    }

    /// <summary>
    /// Garante que o banco de dados está limpo antes de aplicar migrations.
    /// </summary>
    private static async Task EnsureCleanDatabaseAsync(DbContext context, ILogger? logger)
    {
        const int maxRetries = 10;
        var baseDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await context.Database.EnsureDeletedAsync();
                logger?.LogInformation("🧹 Database cleaned (attempt {Attempt})", attempt);
                break;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P03") // database starting up
            {
                if (attempt == maxRetries)
                {
                    logger?.LogError(ex, "❌ PostgreSQL still initializing after {MaxRetries} attempts", maxRetries);
                    throw new InvalidOperationException($"PostgreSQL not ready after {maxRetries} attempts", ex);
                }

                var delay = baseDelay * attempt;
                logger?.LogWarning("⚠️ PostgreSQL initializing... Attempt {Attempt}/{MaxRetries}. Waiting {Delay}s",
                    attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ Failed to clean database: {Message}", ex.Message);
                throw new InvalidOperationException("Failed to clean database before tests", ex);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
        if (_wireMockFixture != null)
            await _wireMockFixture.DisposeAsync();
    }

    /// <summary>
    /// Remove DbContextOptions e DbContext registrations do DI container.
    /// </summary>
    private static void RemoveDbContextRegistrations<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var optionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (optionsDescriptor != null)
            services.Remove(optionsDescriptor);

        var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TContext));
        if (contextDescriptor != null)
            services.Remove(contextDescriptor);
    }

    /// <summary>
    /// Reconfigura os HttpClients dos provedores de CEP para usar o WireMock ao invés das APIs reais.
    /// </summary>
    private void ReconfigureCepProviderClients(IServiceCollection services)
    {
        // Configure HttpClients to point to WireMock
        // AddHttpClient will replace existing registrations if called again
        services.AddHttpClient<ViaCepClient>(client =>
        {
            client.BaseAddress = new Uri(_wireMockFixture!.BaseUrl);
        });

        services.AddHttpClient<BrasilApiCepClient>(client =>
        {
            client.BaseAddress = new Uri(_wireMockFixture!.BaseUrl);
        });

        services.AddHttpClient<OpenCepClient>(client =>
        {
            client.BaseAddress = new Uri(_wireMockFixture!.BaseUrl);
        });

        services.AddHttpClient<IbgeClient>(client =>
        {
            client.BaseAddress = new Uri(_wireMockFixture!.BaseUrl + "/api/v1/localidades/");
        });

        services.AddHttpClient<NominatimClient>(client =>
        {
            client.BaseAddress = new Uri(_wireMockFixture!.BaseUrl);
        });
    }

    /// <summary>
    /// Aplica migrações para um DbContext específico com tratamento de erros padronizado.
    /// Inclui retry logic para erro "57P01" que ocorre quando PostgreSQL termina conexão
    /// após instalar extensões (ex: PostGIS no SearchProviders).
    /// </summary>
    private static async Task ApplyMigrationForContextAsync<TContext>(
        TContext context,
        string moduleName,
        ILogger? logger,
        string? description = null) where TContext : DbContext
    {
        const int maxRetries = 3;
        const int delayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var message = description != null
                    ? $"🔄 Applying {moduleName} module migrations ({description})... (attempt {attempt}/{maxRetries})"
                    : $"🔄 Applying {moduleName} module migrations... (attempt {attempt}/{maxRetries})";
                logger?.LogInformation(message);

                await context.Database.MigrateAsync();
                logger?.LogInformation("✅ {Module} database migrations completed successfully", moduleName);
                return; // Success
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P01" && attempt < maxRetries)
            {
                // 57P01 = "terminating connection due to administrator command"
                // Ocorre quando Postgres reinicia após instalar extensões (ex: PostGIS)
                logger?.LogWarning(
                    "⚠️ PostgreSQL connection terminated (57P01 - extension install). Retrying {Module} migrations... Attempt {Attempt}/{MaxRetries}",
                    moduleName, attempt, maxRetries);
                
                // Aguarda antes de tentar novamente
                await Task.Delay(delayMs * attempt); // Backoff progressivo
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ Failed to apply {Module} migrations: {Message}", moduleName, ex.Message);
                throw new InvalidOperationException($"Failed to apply {moduleName} database migrations", ex);
            }
        }
    }

    /// <summary>
    /// Deserializa resposta JSON usando as opções de serialização compartilhadas (com suporte a enums).
    /// Detecta automaticamente se a resposta está envolvida em um Result&lt;T&gt; e a desembrulha se necessário.
    /// </summary>
    protected async Task<T?> ReadJsonAsync<T>(HttpContent content)
    {
        // Lê tudo como string para evitar problemas de seek em streams não-bufferizados
        var jsonString = await content.ReadAsStringAsync();

        // Tenta deserializar como JsonElement primeiro para inspecionar a estrutura
        try 
        {
            var json = JsonSerializer.Deserialize<JsonElement>(jsonString, SerializationDefaults.Api);
            
            // Verifica se tem as propriedades de um Result
            if (json.ValueKind == JsonValueKind.Object && 
                json.TryGetProperty("isSuccess", out var isSuccessProp) && 
                json.TryGetProperty("value", out var valueProp))
            {
                // É um Result wrapper - verifica se foi sucesso
                if (isSuccessProp.ValueKind == JsonValueKind.False)
                {
                   return default;
                }

                // Se sucesso, desserializa o campo 'value'
                return JsonSerializer.Deserialize<T>(valueProp.GetRawText(), SerializationDefaults.Api);
            }
            
            // Não é wrapper, deserializa direto
            return JsonSerializer.Deserialize<T>(jsonString, SerializationDefaults.Api);
        }
        catch (JsonException)
        {
            // Fallback para deserialização direta se a inspeção falhar (ex: string vazia ou inválida)
            return JsonSerializer.Deserialize<T>(jsonString, SerializationDefaults.Api);
        }
    }

    /// <summary>
    /// Helper para extrair dados da resposta, suportando tanto formato legado (data wrapper) quanto novo (Result with value)
    /// </summary>
    protected static JsonElement GetResponseData(JsonElement response)
    {
        // Se a resposta tem uma propriedade 'value', retorna ela (mesmo que seja null)
        if (response.TryGetProperty("value", out var valueElement))
        {
            return valueElement;
        }

        // Fallback para 'data' (legado) ou retorna a resposta original
        return response.TryGetProperty("data", out var dataElement)
            ? dataElement
            : response;
    }

    /// <summary>
    /// Resolves the ApiService project path using multiple strategies:
    /// 1. Environment variable MEAJUDAAI_API_SERVICE_PATH (for CI override)
    /// 2. Assembly location relative path resolution
    /// 3. Search for .csproj file up the directory tree
    /// </summary>
    private static string? ResolveApiServicePath()
    {
        // Strategy 1: Check environment variable (CI override)
        var envPath = Environment.GetEnvironmentVariable("MEAJUDAAI_API_SERVICE_PATH");
        if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
        {
            Console.WriteLine($"Using ApiService path from environment variable: {envPath}");
            return envPath;
        }

        // Strategy 2: Use assembly location to compute relative path
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        if (!string.IsNullOrEmpty(assemblyDir))
        {
            // From: tests/MeAjudaAi.Integration.Tests/bin/Debug/net10.0/
            // To:   src/Bootstrapper/MeAjudaAi.ApiService/
            var candidatePath = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", "..", "..", "src", "Bootstrapper", "MeAjudaAi.ApiService"));

            if (Directory.Exists(candidatePath))
            {
                Console.WriteLine($"Resolved ApiService path from assembly location: {candidatePath}");
                return candidatePath;
            }
        }

        // Strategy 3: Search for .csproj file up the directory tree (fallback)
        var currentDir = assemblyDir;
        while (!string.IsNullOrEmpty(currentDir))
        {
            var projectFile = Path.Combine(currentDir, "src", "Bootstrapper", "MeAjudaAi.ApiService", "MeAjudaAi.ApiService.csproj");
            if (File.Exists(projectFile))
            {
                var resolvedPath = Path.GetDirectoryName(projectFile);
                Console.WriteLine($"Found ApiService path via directory search: {resolvedPath}");
                return resolvedPath;
            }

            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        Console.Error.WriteLine("ERROR: Could not resolve ApiService path using any strategy.");
        Console.Error.WriteLine($"Assembly location: {assemblyLocation}");
        Console.Error.WriteLine($"Environment variable MEAJUDAAI_API_SERVICE_PATH: {envPath ?? "(not set)"}");

        return null;
    }
}
