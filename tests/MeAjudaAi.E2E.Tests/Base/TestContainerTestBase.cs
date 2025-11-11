using Bogus;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private WebApplicationFactory<Program> _factory = null!;

    protected HttpClient ApiClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    protected static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    public virtual async ValueTask InitializeAsync()
    {
        // Configurar containers com configuração mais robusta
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:13-alpine")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        // Iniciar containers
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();

        // Configurar WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _postgresContainer.GetConnectionString(),
                        ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString(),
                        ["Logging:LogLevel:Default"] = "Warning",
                        ["Logging:LogLevel:Microsoft"] = "Error",
                        ["Logging:LogLevel:Microsoft.EntityFrameworkCore"] = "Error",
                        ["RabbitMQ:Enabled"] = "false",
                        ["Keycloak:Enabled"] = "false",
                        ["Cache:Enabled"] = "false", // Disable Redis for now
                        ["Cache:ConnectionString"] = _redisContainer.GetConnectionString(),
                        // Desabilitar completamente Rate Limiting nos testes E2E
                        ["AdvancedRateLimit:General:Enabled"] = "false",
                        // Valores de fallback muito altos caso não consiga desabilitar
                        ["AdvancedRateLimit:Anonymous:RequestsPerMinute"] = "999999",
                        ["AdvancedRateLimit:Anonymous:RequestsPerHour"] = "999999",
                        ["AdvancedRateLimit:Anonymous:RequestsPerDay"] = "999999",
                        ["AdvancedRateLimit:Authenticated:RequestsPerMinute"] = "999999",
                        ["AdvancedRateLimit:Authenticated:RequestsPerHour"] = "999999",
                        ["AdvancedRateLimit:Authenticated:RequestsPerDay"] = "999999",
                        ["AdvancedRateLimit:General:WindowInSeconds"] = "3600",
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

                    // Remover configuração existente do DbContext
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Reconfigurar DbContext com TestContainer connection string
                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString())
                               .UseSnakeCaseNamingConvention()
                               .EnableSensitiveDataLogging(false)
                               .ConfigureWarnings(warnings =>
                                   warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    // Reconfigurar ProvidersDbContext com TestContainer connection string
                    var providersDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProvidersDbContext>));
                    if (providersDescriptor != null)
                        services.Remove(providersDescriptor);

                    services.AddDbContext<ProvidersDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString())
                               .UseSnakeCaseNamingConvention()
                               .EnableSensitiveDataLogging(false)
                               .ConfigureWarnings(warnings =>
                                   warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    // Substituir IKeycloakService por MockKeycloakService para testes
                    var keycloakDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IKeycloakService));
                    if (keycloakDescriptor != null)
                        services.Remove(keycloakDescriptor);

                    services.AddScoped<IKeycloakService, MockKeycloakService>();

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

        ApiClient = _factory.CreateClient();

        // Aplicar migrações diretamente no banco TestContainer
        await ApplyMigrationsAsync();

        // Aguardar API ficar disponível
        await WaitForApiHealthAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        ApiClient?.Dispose();
        _factory?.Dispose();

        if (_postgresContainer != null)
            await _postgresContainer.StopAsync();

        if (_redisContainer != null)
            await _redisContainer.StopAsync();
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
            catch (Exception ex) when (attempt < maxAttempts)
            {
                if (attempt == maxAttempts)
                {
                    throw new InvalidOperationException($"API não respondeu após {maxAttempts} tentativas: {ex.Message}");
                }
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
    }

    // Helper methods usando serialização compartilhada
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PostAsync(requestUri, stringContent);
    }

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, JsonOptions);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        return await ApiClient.PutAsync(requestUri, stringContent);
    }

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
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    /// <summary>
    /// Configura autenticação como usuário regular para testes
    /// </summary>
    protected static void AuthenticateAsUser(string userId = "test-user-id", string username = "testuser")
    {
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userId, username);
    }

    /// <summary>
    /// Remove autenticação (testes anônimos)
    /// </summary>
    protected static void AuthenticateAsAnonymous()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(Uri requestUri, T content)
        => await PostJsonAsync(requestUri.ToString(), content);

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(Uri requestUri, T content)
        => await PutJsonAsync(requestUri.ToString(), content);
}
