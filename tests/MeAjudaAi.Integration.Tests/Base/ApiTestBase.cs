using System.Reflection;
using System.Text.Json;
using MeAjudaAi.ApiService;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Auth;
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
        Environment.SetEnvironmentVariable("Locations__ExternalApis__IBGE__BaseUrl", $"{wireMockUrl}/api/v1/localidades/");

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
                        // Geographic restriction: Only specific cities allowed, NOT entire states
                        // This ensures fallback validation properly blocks non-allowed cities in same state
                        ["GeographicRestriction:AllowedStates:0"] = "MG",
                        ["GeographicRestriction:AllowedStates:1"] = "ES",
                        ["GeographicRestriction:AllowedStates:2"] = "RJ",
                        ["GeographicRestriction:AllowedCities:0"] = "Muriaé",
                        ["GeographicRestriction:AllowedCities:1"] = "Itaperuna",
                        ["GeographicRestriction:AllowedCities:2"] = "Linhares",
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

                    // Adiciona mocks de serviços para testes
                    services.AddDocumentsTestServices();

                    // Conditionally replace geographic validation with mock
                    // IBGE-focused tests can override UseMockGeographicValidation to use real service with WireMock
                    if (UseMockGeographicValidation)
                    {
                        var geoValidationDescriptor = services.FirstOrDefault(d =>
                            d.ServiceType == typeof(IGeographicValidationService));
                        if (geoValidationDescriptor != null)
                            services.Remove(geoValidationDescriptor);

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
        var logger = scope.ServiceProvider.GetService<ILogger<ApiTestBase>>();

        // Aplica migrações exatamente como nos testes E2E
        await ApplyMigrationsAsync(usersContext, providersContext, documentsContext, catalogsContext, locationsContext, logger);

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
                await locationsContext.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO locations.allowed_cities (""Id"", ""IbgeCode"", ""CityName"", ""StateSigla"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"", ""CreatedBy"", ""UpdatedBy"") 
                      VALUES (gen_random_uuid(), {0}, {1}, {2}, true, {3}, {4}, 'system', NULL)",
                    city.IbgeCode, city.CityName, city.State, DateTime.UtcNow, DateTime.UtcNow);
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "23505") // 23505 = unique violation
            {
                // Ignore duplicate key errors - city already exists
                logger?.LogDebug("City {City}/{State} already exists, skipping", city.CityName, city.State);
            }
        }

        logger?.LogInformation("✅ Seeded {Count} test cities into allowed_cities table", testCities.Length);
    }

    private static async Task ApplyMigrationsAsync(
        UsersDbContext usersContext,
        ProvidersDbContext providersContext,
        DocumentsDbContext documentsContext,
        ServiceCatalogsDbContext catalogsContext,
        LocationsDbContext locationsContext,
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
                logger?.LogInformation("🧹 Banco de dados existente limpo (tentativa {Attempt})", attempt);
                break; // Sucesso, sai do loop
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "57P03") // 57P03 = database starting up
            {
                if (attempt == maxRetries)
                {
                    logger?.LogError(ex, "❌ PostgreSQL ainda iniciando após {MaxRetries} tentativas", maxRetries);
                    var totalWaitTime = maxRetries * (maxRetries + 1) / 2; // Sum: 1+2+3+...+10 = 55 seconds
                    throw new InvalidOperationException($"PostgreSQL não ficou pronto após {maxRetries} tentativas (~{totalWaitTime} segundos)", ex);
                }

                var delay = baseDelay * attempt; // Linear backoff: 1s, 2s, 3s, etc.
                logger?.LogWarning(
                    "⚠️ PostgreSQL iniciando... Tentativa {Attempt}/{MaxRetries}. Aguardando {Delay}s",
                    attempt, maxRetries, delay.TotalSeconds);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "❌ Falha crítica ao limpar banco existente: {Message}", ex.Message);
                throw new InvalidOperationException("Não foi possível limpar o banco de dados antes dos testes", ex);
            }
        }

        // Aplica migrações em todos os módulos
        await ApplyMigrationForContextAsync(usersContext, "Users", logger, "UsersDbContext primeiro (cria database e schema users)");
        await ApplyMigrationForContextAsync(providersContext, "Providers", logger, "ProvidersDbContext (banco já existe, só precisa do schema providers)");
        await ApplyMigrationForContextAsync(documentsContext, "Documents", logger, "DocumentsDbContext (banco já existe, só precisa do schema documents)");
        await ApplyMigrationForContextAsync(catalogsContext, "ServiceCatalogs", logger, "ServiceCatalogsDbContext (banco já existe, só precisa do schema service_catalogs)");
        await ApplyMigrationForContextAsync(locationsContext, "Locations", logger, "LocationsDbContext (banco já existe, só precisa do schema locations)");

        // Verifica se as tabelas existem
        await VerifyContextAsync(usersContext, "Users", () => usersContext.Users.CountAsync(), logger);
        await VerifyContextAsync(providersContext, "Providers", () => providersContext.Providers.CountAsync(), logger);
        await VerifyContextAsync(documentsContext, "Documents", () => documentsContext.Documents.CountAsync(), logger);
        await VerifyContextAsync(catalogsContext, "ServiceCatalogs", () => catalogsContext.ServiceCategories.CountAsync(), logger);
        await VerifyContextAsync(locationsContext, "Locations", () => locationsContext.AllowedCities.CountAsync(), logger);
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
                ? $"🔄 Aplicando migrações do módulo {moduleName} ({description})..."
                : $"🔄 Aplicando migrações do módulo {moduleName}...";
            logger?.LogInformation(message);

            await context.Database.MigrateAsync();
            logger?.LogInformation("✅ Migrações do banco {Module} completadas com sucesso", moduleName);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Falha ao aplicar migrações do {Module}: {Message}", moduleName, ex.Message);
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
            logger?.LogInformation("Verificação do banco {Module} bem-sucedida - Contagem: {Count}", moduleName, count);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Verificação do banco {Module} falhou", moduleName);
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
