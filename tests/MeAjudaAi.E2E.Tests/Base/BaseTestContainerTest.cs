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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    /// <summary>
    /// Sobrescreva em classes derivadas para habilitar o message bus em memória síncrono e eventos de domínio.
    /// Usado para testes que dependem de eventos de integração entre módulos (ex: Ratings -> SearchProviders).
    /// </summary>
    protected virtual bool EnableEventsAndMessageBus => false;

    private WebApplicationFactory<Program> _factory = null!;

    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    protected static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public virtual async ValueTask InitializeAsync()
    {
        // 1. Inicialização centralizada (Containers + Migrações + Limpeza Global)
        await E2EStabilityCoordinator.EnsureInitializedAsync();

        // 2. Configurar WebApplicationFactory (instância por teste, mas usa containers compartilhados)
        InitializeFactory();

        // 3. Criar cliente HTTP com injeção de header de contexto de teste
        ApiClient = _factory.CreateDefaultClient(new TestContextHeaderHandler());

        // 4. Aguardar API ficar disponível
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
                        ["ConnectionStrings:DefaultConnection"] = SharedTestContainers.PostgresConnectionString,
                        ["ConnectionStrings:meajudaai-db"] = SharedTestContainers.PostgresConnectionString,
                        ["ConnectionStrings:UsersDb"] = SharedTestContainers.PostgresConnectionString,
                        ["ConnectionStrings:ProvidersDb"] = SharedTestContainers.PostgresConnectionString,
                        ["ConnectionStrings:RatingsDb"] = SharedTestContainers.PostgresConnectionString,
                        ["ConnectionStrings:DocumentsDb"] = SharedTestContainers.PostgresConnectionString,
                        ["ConnectionStrings:Redis"] = SharedTestContainers.RedisConnectionString,
                        ["Azure:Storage:ConnectionString"] = SharedTestContainers.AzuriteConnectionString,
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
                        ["Cache:ConnectionString"] = SharedTestContainers.RedisConnectionString,
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
                    // Remover background workers que interferem com migrations e isolamento
                    var hostedServices = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
                    foreach (var service in hostedServices)
                    {
                        services.Remove(service);
                    }

                    // Configurar logging para capturar logs de E2E apenas se solicitado
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        if (Environment.GetEnvironmentVariable("TEST_ENABLE_SENSITIVE_LOGS") == "true")
                        {
                            logging.AddConsole();
                            logging.SetMinimumLevel(LogLevel.Debug);
                        }
                        else
                        {
                            logging.SetMinimumLevel(LogLevel.Warning);
                        }
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
                    ReconfigureDbContext<MeAjudaAi.Modules.Payments.Infrastructure.Persistence.PaymentsDbContext>(services);

                    // Configurar PostgresOptions e Dapper para SearchProviders
                    var postgresOptionsDescriptors = services.Where(d => d.ServiceType == typeof(PostgresOptions)).ToList();
                    foreach (var descriptor in postgresOptionsDescriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddSingleton(new PostgresOptions
                    {
                        ConnectionString = SharedTestContainers.PostgresConnectionString
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
        // Limpar contexto de autenticação para evitar poluição entre testes
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        ApiClient?.Dispose();
        if (_factory != null) await _factory.DisposeAsync();

        // NÃO fazer dispose dos containers aqui pois são singletons estáticos compartilhados
        // Serão limpos pelo Ryuk (Testcontainers) quando o processo terminar
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
                // Continuar para próxima tentativa
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException($"A API não ficou saudável após {maxAttempts} tentativas");
    }

    protected async Task CleanupDatabaseAsync()
    {
        await E2EStabilityCoordinator.GlobalCleanupAsync();
        await SharedTestContainers.FlushRedisAsync();
    }


    // Métodos auxiliares usando serialização compartilhada
#pragma warning disable CA2000 // Dispose StringContent - tratado pelo HttpClient
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

    protected async Task<HttpResponseMessage> PatchJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await ApiClient.PatchAsync(requestUri, stringContent);
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
    protected static void AuthenticateAsUser(string userId = "00000000-0000-0000-0000-000000000002", string username = "testuser")
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

    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(SharedTestContainers.PostgresConnectionString, npgsqlOptions =>
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
                .EnableSensitiveDataLogging(Environment.GetEnvironmentVariable("TEST_ENABLE_SENSITIVE_LOGS") == "true")
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
