using Bogus;
using MeAjudaAi.ApiService;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests.Mocks;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
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

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Base class para testes E2E usando TestContainers
/// Isolada de Aspire, com infraestrutura própria de teste
/// </summary>
public abstract class TestContainerTestBase : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;
    private RedisContainer _redisContainer = null!;
    private AzuriteContainer _azuriteContainer = null!;
    private WebApplicationFactory<Program> _factory = null!;

    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    protected static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    // Nota: Removido construtor estático com SetAllowUnauthenticated(true)
    // Testes agora devem configurar explicitamente autenticação via AuthenticateAsAdmin(), AuthenticateAsUser(), etc.
    // Isso previne race conditions onde testes esperam permissões específicas mas recebem acesso admin

    public virtual async ValueTask InitializeAsync()
    {
        // Configurar containers com configuração mais robusta
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:16-3.4") // Mesma imagem usada em CI/CD e compose
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .WithCleanUp(true)
            .Build();

        // Iniciar containers
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
        await _azuriteContainer.StartAsync();

        // Configurar WebApplicationFactory
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
                        ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:meajudaai-db"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:UsersDb"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:ProvidersDb"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:DocumentsDb"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString(),
                        ["Azure:Storage:ConnectionString"] = _azuriteContainer.GetConnectionString(),
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
                        ["Cache:Enabled"] = "false", // Desabilitar Redis por enquanto
                        ["Cache:ConnectionString"] = _redisContainer.GetConnectionString(),
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
                    ReconfigureDbContext<DocumentsDbContext>(services);
                    ReconfigureDbContext<ServiceCatalogsDbContext>(services);
                    ReconfigureSearchProvidersDbContext(services);

                    // Configurar PostgresOptions e Dapper para SearchProviders
                    var postgresOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(PostgresOptions));
                    if (postgresOptionsDescriptor != null)
                        services.Remove(postgresOptionsDescriptor);

                    services.AddSingleton(new PostgresOptions
                    {
                        ConnectionString = _postgresContainer.GetConnectionString()
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

                    // Substituir IMessageBus por MockMessageBus para testes (RabbitMQ desabilitado)
                    // Remover TODAS as implementações existentes de IMessageBus e adicionar o mock
                    var messageBusDescriptors = services.Where(d => d.ServiceType == typeof(MeAjudaAi.Shared.Messaging.IMessageBus)).ToList();
                    foreach (var descriptor in messageBusDescriptors)
                    {
                        services.Remove(descriptor);
                    }
                    
                    // Adicionar como Singleton para garantir que a mesma instância seja usada em todos os lugares
                    services.AddSingleton<MeAjudaAi.Shared.Messaging.IMessageBus, MockMessageBus>();

                    // Substituir IDomainEventProcessor por um mock que não processa eventos (evitar publicação de eventos de integração em E2E)
                    var domainEventProcessorDescriptors = services.Where(d => d.ServiceType == typeof(MeAjudaAi.Shared.Events.IDomainEventProcessor)).ToList();
                    foreach (var descriptor in domainEventProcessorDescriptors)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddScoped<MeAjudaAi.Shared.Events.IDomainEventProcessor, MockDomainEventProcessor>();

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

                    // Configurar aplicação automática de migrações apenas para testes
                    services.AddScoped<Func<UsersDbContext>>(provider => () =>
                    {
                        var context = provider.GetRequiredService<UsersDbContext>();
                        // Aplicar migrações apenas em testes
                        context.Database.Migrate();
                        return context;
                    });

                    services.AddScoped<Func<ProvidersDbContext>>(provider => () =>
                    {
                        var context = provider.GetRequiredService<ProvidersDbContext>();
                        // Migrations are applied explicitly in ApplyMigrationsAsync, no action needed here
                        return context;
                    });
                });
            });

        // Create HTTP client with test context header injection
        ApiClient = _factory.CreateDefaultClient(new TestContextHeaderHandler());

        // Aplicar migrações diretamente no banco TestContainer
        await ApplyMigrationsAsync();

        // Aguardar API ficar disponível
        await WaitForApiHealthAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        // Clear authentication context to prevent state pollution between tests
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        ApiClient?.Dispose();
        _factory?.Dispose();

        if (_postgresContainer != null)
            await _postgresContainer.StopAsync();

        if (_redisContainer != null)
            await _redisContainer.StopAsync();

        if (_azuriteContainer != null)
            await _azuriteContainer.StopAsync();
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

        throw new InvalidOperationException($"API não ficou saudável após {maxAttempts} tentativas");
    }

    private async Task ApplyMigrationsAsync()
    {
        using var scope = _factory.Services.CreateScope();

        // Garantir que o banco está limpo primeiro
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await usersContext.Database.EnsureDeletedAsync();

        // Aplicar migrações no UsersDbContext (isso cria o banco e o schema users)
        await usersContext.Database.MigrateAsync();

        // Para ProvidersDbContext, só aplicar migrações (o banco já existe, só precisamos do schema providers)
        var providersContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        await providersContext.Database.MigrateAsync();

        // Para DocumentsDbContext, só aplicar migrações (o banco já existe, só precisamos do schema documents)
        var documentsContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        await documentsContext.Database.MigrateAsync();

        // Para ServiceCatalogsDbContext, só aplicar migrações (o banco já existe, só precisamos do schema catalogs)
        var catalogsContext = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
        await catalogsContext.Database.MigrateAsync();

        // Para SearchProvidersDbContext, só aplicar migrações (o banco já existe, só precisamos do schema search + PostGIS)
        var searchContext = scope.ServiceProvider.GetRequiredService<SearchProvidersDbContext>();
        await searchContext.Database.MigrateAsync();

        // Para LocationsDbContext, só aplicar migrações (o banco já existe, só precisamos do schema locations)
        var locationsContext = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
        await locationsContext.Database.MigrateAsync();
    }

    // Helper methods usando serialização compartilhada
#pragma warning disable CA2000 // Dispose StringContent - handled by HttpClient
    /// <summary>
    /// Sends a POST request with JSON content to the specified URI.
    /// </summary>
    /// <typeparam name="T">The type of the content to serialize.</typeparam>
    /// <param name="requestUri">The URI to send the request to.</param>
    /// <param name="content">The content to serialize and send.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PostAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Sends a PUT request with JSON content to the specified URI.
    /// </summary>
    /// <typeparam name="T">The type of the content to serialize.</typeparam>
    /// <param name="requestUri">The URI to send the request to.</param>
    /// <param name="content">The content to serialize and send.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PutAsync(requestUri, stringContent);
    }
#pragma warning restore CA2000

    /// <summary>
    /// Deserializes JSON content from an HTTP response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response containing JSON content.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
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
        var contextId = ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    /// <summary>
    /// Configura autenticação como usuário regular para testes
    /// </summary>
    protected static void AuthenticateAsUser(string userId = "test-user-id", string username = "testuser")
    {
        var contextId = ConfigurableTestAuthenticationHandler.GetOrCreateTestContext();
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
    /// Sends a POST request with JSON content to the specified URI.
    /// </summary>
    /// <typeparam name="T">The type of the content to serialize.</typeparam>
    /// <param name="requestUri">The URI to send the request to.</param>
    /// <param name="content">The content to serialize and send.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T content)
        => await PostJsonAsync(requestUri.ToString(), content);

    /// <summary>
    /// Sends a PUT request with JSON content to the specified URI.
    /// </summary>
    /// <typeparam name="T">The type of the content to serialize.</typeparam>
    /// <param name="requestUri">The URI to send the request to.</param>
    /// <param name="content">The content to serialize and send.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> PutJsonAsync<T>(Uri requestUri, T content)
        => await PutJsonAsync(requestUri.ToString(), content);

    /// <summary>
    /// Reconfigura um DbContext para usar a connection string do TestContainer
    /// </summary>
    private void ReconfigureDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(_postgresContainer.GetConnectionString())
                   .UseSnakeCaseNamingConvention()
                   .EnableSensitiveDataLogging(true)  // Useful for test debugging
                   .ConfigureWarnings(warnings =>
                       warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });
    }

    /// <summary>
    /// Reconfigura SearchProvidersDbContext com suporte PostGIS/NetTopologySuite
    /// </summary>
    private void ReconfigureSearchProvidersDbContext(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SearchProvidersDbContext>));
        if (descriptor != null)
            services.Remove(descriptor);

        services.AddDbContext<SearchProvidersDbContext>(options =>
        {
            options.UseNpgsql(
                _postgresContainer.GetConnectionString(),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite(); // Habilitar suporte PostGIS
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search");
                })
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging(false)
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
    /// Mock implementation of IMessageBus for E2E tests
    /// </summary>
    private class MockMessageBus : MeAjudaAi.Shared.Messaging.IMessageBus
    {
        public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
        {
            // No-op: messages are not actually sent in E2E tests
            return Task.CompletedTask;
        }

        public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
        {
            // No-op: events are not actually published in E2E tests
            return Task.CompletedTask;
        }

        public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
        {
            // No-op: subscriptions are not actually created in E2E tests
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mock implementation of IDomainEventProcessor for E2E tests
    /// Does not process domain events to avoid integration event publication
    /// </summary>
    private class MockDomainEventProcessor : MeAjudaAi.Shared.Events.IDomainEventProcessor
    {
        public Task ProcessDomainEventsAsync(IEnumerable<MeAjudaAi.Shared.Events.IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
        {
            // No-op: domain events are not processed in E2E tests to avoid integration event publication
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Mock implementation of IUserDomainService for E2E tests
    /// </summary>
    private class MockUserDomainService : MeAjudaAi.Modules.Users.Domain.Services.IUserDomainService
    {
        public Task<MeAjudaAi.Shared.Functional.Result<MeAjudaAi.Modules.Users.Domain.Entities.User>> CreateUserAsync(
            MeAjudaAi.Modules.Users.Domain.ValueObjects.Username username,
            MeAjudaAi.Modules.Users.Domain.ValueObjects.Email email,
            string firstName,
            string lastName,
            string password,
            IEnumerable<string> roles,
            string? phoneNumber = null,
            CancellationToken cancellationToken = default)
        {
            // Para testes, criar usuário mock
            var user = new MeAjudaAi.Modules.Users.Domain.Entities.User(username, email, firstName, lastName, $"keycloak_{Guid.NewGuid()}", phoneNumber);
            return Task.FromResult(MeAjudaAi.Shared.Functional.Result<MeAjudaAi.Modules.Users.Domain.Entities.User>.Success(user));
        }

        public Task<MeAjudaAi.Shared.Functional.Result> SyncUserWithKeycloakAsync(
            MeAjudaAi.Modules.Users.Domain.ValueObjects.UserId userId, 
            CancellationToken cancellationToken = default)
        {
            // Para testes, simular sincronização bem-sucedida
            return Task.FromResult(MeAjudaAi.Shared.Functional.Result.Success());
        }
    }
}
