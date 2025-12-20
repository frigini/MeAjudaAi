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
using MeAjudaAi.Shared.Tests.Mocks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Classe base unificada para testes de integração com suporte a autenticação baseada em instância.
/// Elimina condições de corrida e instabilidade causadas por estado estático.
/// Cria containers individuais para máxima compatibilidade com CI.
/// Suporte completo a 6 módulos: Users, Providers, Documents, ServiceCatalogs, Locations, SearchProviders.
/// </summary>
public abstract class ApiTestBase : IAsyncLifetime
{
    private SimpleDatabaseFixture? _databaseFixture;
    private WireMockFixture? _wireMockFixture;
    private WebApplicationFactory<Program>? _factory;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _factory!.Services;
    protected ITestAuthenticationConfiguration AuthConfig { get; private set; } = null!;
    protected WireMockFixture WireMock => _wireMockFixture ?? throw new InvalidOperationException("WireMock not initialized");

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
                    // TODO: Investigate Azurite SAS token issue and migrate from Mock to Azurite emulator
                    // Currently using Mock because Azurite throws 500 errors on upload tests (CanGenerateSasUri problem).
                    // See tracking issue: https://github.com/your-repo/issues/XXX
                    // Investigation steps: check Azurite logs, test container creation manually, verify Azurite 3.33.0 compatibility with SAS tokens
                    services.AddDocumentsTestServices(connectionString: string.Empty, useAzurite: false);

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

        // Aplica migrações do banco de dados para testes
        // Nota: Todos os módulos usam setup baseado em migrações para consistência com produção
        using var scope = _factory.Services.CreateScope();
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var providersContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var documentsContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var catalogsContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
        var locationsContext = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        var searchProvidersContext = scope.ServiceProvider.GetRequiredService<SearchProvidersDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApiTestBase>>();

        // Aplica migrações exatamente como nos testes E2E
        await ApplyMigrationsAsync(usersContext, providersContext, documentsContext, catalogsContext, locationsContext, searchProvidersContext, logger);

        // Seed test data for allowed cities (required for GeographicRestriction tests)
        await SeedTestDataAsync(locationsContext, logger);
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

        foreach (var city in testCities)
        {
            try
            {
                // Check if city already exists to avoid duplicate key errors
                var exists = await locationsContext.AllowedCities
                    .AnyAsync(c => c.CityName == city.CityName && c.StateSigla == city.State);
                
                if (exists)
                {
                    logger?.LogDebug("City {City}/{State} already exists, skipping", city.CityName, city.State);
                    continue;
                }

                // Use EF Core entity instead of raw SQL to avoid case sensitivity issues
                var allowedCity = new MeAjudaAi.Modules.Locations.Domain.Entities.AllowedCity(
                    city.CityName,
                    city.State,
                    "system",
                    city.IbgeCode);
                
                locationsContext.AllowedCities.Add(allowedCity);
                await locationsContext.SaveChangesAsync();
                
                logger?.LogDebug("✅ Seeded city {City}/{State} (IBGE: {IbgeCode})", city.CityName, city.State, city.IbgeCode);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ Failed to seed city {City}/{State}: {Message}", city.CityName, city.State, ex.Message);
                // Clear the change tracker to recover from errors
                locationsContext.ChangeTracker.Clear();
            }
        }

        var totalCount = await locationsContext.AllowedCities.CountAsync();
        logger?.LogInformation("✅ Seeded test cities. Total cities in database: {Count}", totalCount);
    }

    private static async Task ApplyMigrationsAsync(
        UsersDbContext usersContext,
        ProvidersDbContext providersContext,
        DocumentsDbContext documentsContext,
        ServiceCatalogsDbContext catalogsContext,
        LocationsDbContext locationsContext,
        SearchProvidersDbContext searchProvidersContext,
        ILogger? logger)
    {
        // Garante estado limpo do banco de dados (como nos testes E2E)
        // Com retry para evitar race condition "database system is starting up"
        const int maxRetries = 10;
        var baseDelay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await usersContext.Database.EnsureDeletedAsync();
                logger?.LogInformation("🧹 Existing database cleaned (attempt {Attempt})", attempt);
                break; // Sucesso, sai do loop
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P03") // 57P03 = database starting up
            {
                if (attempt == maxRetries)
                {
                    logger?.LogError(ex, "❌ PostgreSQL still initializing after {MaxRetries} attempts", maxRetries);
                    var totalWaitTime = maxRetries * (maxRetries + 1) / 2; // Sum: 1+2+3+...+10 = 55 seconds
                    throw new InvalidOperationException($"PostgreSQL não ficou pronto após {maxRetries} tentativas (~{totalWaitTime} segundos)", ex);
                }

                var delay = baseDelay * attempt; // Linear backoff: 1s, 2s, 3s, etc.
                logger?.LogWarning(
                    "⚠️ PostgreSQL initializing... Attempt {Attempt}/{MaxRetries}. Waiting {Delay}s",
                    attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ Critical failure cleaning existing database: {Message}", ex.Message);
                throw new InvalidOperationException("Não foi possível limpar o banco de dados antes dos testes", ex);
            }
        }

        // Aplica migrações em todos os módulos
        await ApplyMigrationForContextAsync(usersContext, "Users", logger, "UsersDbContext primeiro (cria database e schema users)");
        await ApplyMigrationForContextAsync(providersContext, "Providers", logger, "ProvidersDbContext (banco já existe, só precisa do schema providers)");
        await ApplyMigrationForContextAsync(documentsContext, "Documents", logger, "DocumentsDbContext (banco já existe, só precisa do schema documents)");
        await ApplyMigrationForContextAsync(catalogsContext, "ServiceCatalogs", logger, "ServiceCatalogsDbContext (banco já existe, só precisa do schema service_catalogs)");
        await ApplyMigrationForContextAsync(locationsContext, "Locations", logger, "LocationsDbContext (banco já existe, só precisa do schema locations)");
        await ApplyMigrationForContextAsync(searchProvidersContext, "SearchProviders", logger, "SearchProvidersDbContext (banco já existe, só precisa do schema search_providers)");

        // Verifica se as tabelas existem
        await VerifyContextAsync(usersContext, "Users", () => usersContext.Users.CountAsync(), logger);
        await VerifyContextAsync(providersContext, "Providers", () => providersContext.Providers.CountAsync(), logger);
        await VerifyContextAsync(documentsContext, "Documents", () => documentsContext.Documents.CountAsync(), logger);
        await VerifyContextAsync(catalogsContext, "ServiceCatalogs", () => catalogsContext.ServiceCategories.CountAsync(), logger);
        await VerifyContextAsync(locationsContext, "Locations", () => locationsContext.AllowedCities.CountAsync(), logger);
        await VerifyContextAsync(searchProvidersContext, "SearchProviders", () => searchProvidersContext.SearchableProviders.CountAsync(), logger);
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
    /// </summary>
    private static async Task ApplyMigrationForContextAsync<TContext>(
        TContext context,
        string moduleName,
        ILogger? logger,
        string? description = null) where TContext : DbContext
    {
        try
        {
            var message = description != null
                ? $"🔄 Applying {moduleName} module migrations ({description})..."
                : $"🔄 Applying {moduleName} module migrations...";
            logger?.LogInformation(message);

            await context.Database.MigrateAsync();
            logger?.LogInformation("✅ {Module} database migrations completed successfully", moduleName);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Failed to apply {Module} migrations: {Message}", moduleName, ex.Message);
            throw new InvalidOperationException($"Não foi possível aplicar migrações do banco {moduleName}", ex);
        }
    }

    /// <summary>
    /// Verifica se um DbContext está funcionando corretamente executando uma query de contagem.
    /// </summary>
    private static async Task VerifyContextAsync<TContext>(
        TContext context,
        string moduleName,
        Func<Task<int>> countQuery,
        ILogger? logger) where TContext : DbContext
    {
        try
        {
            var count = await countQuery();
            logger?.LogInformation("{Module} database verification successful - Count: {Count}", moduleName, count);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "{Module} database verification failed", moduleName);
            throw new InvalidOperationException($"Banco {moduleName} não foi inicializado corretamente", ex);
        }
    }

    /// <summary>
    /// Deserializa resposta JSON usando as opções de serialização compartilhadas (com suporte a enums).
    /// </summary>
    protected async Task<T?> ReadJsonAsync<T>(HttpContent content)
    {
        var stream = await content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(stream, SerializationDefaults.Api);
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
