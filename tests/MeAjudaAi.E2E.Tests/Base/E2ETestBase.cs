using System.Net.Http.Json;
using Bogus;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Classe base otimizada para todos os testes E2E
/// Combina funcionalidades de TestContainers com configuração simplificada
/// Utiliza TestContainers para PostgreSQL e Redis com serialização padronizada
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private WebApplicationFactory<Program>? _factory;

    protected HttpClient HttpClient { get; private set; } = null!;
    protected Faker Faker { get; } = new();

    /// <summary>
    /// Opções de serialização JSON padrão do sistema
    /// </summary>
    protected static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    /// <summary>
    /// Indica se este teste precisa do Redis (padrão: false para performance)
    /// </summary>
    protected virtual bool RequiresRedis => false;

    /// <summary>
    /// Configurações específicas do teste
    /// </summary>
    protected virtual Dictionary<string, string?> GetTestConfiguration()
    {
        var config = new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", _postgresContainer?.GetConnectionString()},
            {"ConnectionStrings:meajudaai-db-local", _postgresContainer?.GetConnectionString()},
            {"ConnectionStrings:users-db", _postgresContainer?.GetConnectionString()},
            {"Postgres:ConnectionString", _postgresContainer?.GetConnectionString()},
            {"ASPNETCORE_ENVIRONMENT", "Testing"},
            {"Logging:LogLevel:Default", "Warning"},
            {"Logging:LogLevel:Microsoft", "Error"},
            {"Logging:LogLevel:Microsoft.AspNetCore", "Error"},
            {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Error"},
            // Desabilita serviços não necessários para testes
            {"Messaging:Enabled", "false"},
            {"Cache:WarmupEnabled", "false"},
            {"ServiceBus:Enabled", "false"},
            {"Keycloak:Enabled", "false"},
            // Configuração de Rate Limiting para testes - valores muito altos para evitar bloqueios
            {"AdvancedRateLimit:Anonymous:RequestsPerMinute", "10000"},
            {"AdvancedRateLimit:Anonymous:RequestsPerHour", "100000"},
            {"AdvancedRateLimit:Anonymous:RequestsPerDay", "1000000"},
            {"AdvancedRateLimit:Authenticated:RequestsPerMinute", "10000"},
            {"AdvancedRateLimit:Authenticated:RequestsPerHour", "100000"},
            {"AdvancedRateLimit:Authenticated:RequestsPerDay", "1000000"},
            {"AdvancedRateLimit:General:WindowInSeconds", "60"},
            {"AdvancedRateLimit:General:EnableIpWhitelist", "false"},
            // Configuração legada também para garantir
            {"RateLimit:DefaultRequestsPerMinute", "10000"},
            {"RateLimit:AuthRequestsPerMinute", "10000"},
            {"RateLimit:SearchRequestsPerMinute", "10000"},
            {"RateLimit:WindowInSeconds", "60"}
        };

        if (RequiresRedis && _redisContainer != null)
        {
            config["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString();
            config["Cache:Enabled"] = "true";
        }
        else
        {
            config["Cache:Enabled"] = "false";
        }

        return config;
    }

    public virtual async ValueTask InitializeAsync()
    {
        // Configura e inicia PostgreSQL (obrigatório)
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();

        // Configura Redis apenas se necessário
        if (RequiresRedis)
        {
            _redisContainer = new RedisBuilder()
                .WithImage("redis:7-alpine")
                .WithCleanUp(true)
                .Build();

            await _redisContainer.StartAsync();
        }

        // Configura WebApplicationFactory
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(GetTestConfiguration());
                });

                builder.ConfigureServices(services =>
                {
                    // Remove serviços hospedados problemáticos
                    var hostedServices = services
                        .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                        .ToList();

                    foreach (var service in hostedServices)
                    {
                        services.Remove(service);
                    }

                    // Configura autenticação de teste
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, ConfigurableTestAuthenticationHandler>(
                            "Test", options => { });

                    // Reconfigura DbContext com connection string do container
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(_postgresContainer.GetConnectionString())
                              .EnableSensitiveDataLogging(false)
                              .LogTo(_ => { }, LogLevel.Error); // Minimal logging
                    });

                    // Configura logging mínimo
                    services.Configure<HostOptions>(options =>
                    {
                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });
            });

        HttpClient = _factory.CreateClient();

        // Aguarda inicialização da aplicação
        await WaitForApplicationStartup();

        // Aplica migrações do banco de dados
        await EnsureDatabaseSchemaAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();
        _factory?.Dispose();

        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }

        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Aguarda a aplicação inicializar completamente
    /// </summary>
    protected virtual async Task WaitForApplicationStartup()
    {
        var maxAttempts = 30;
        var delay = TimeSpan.FromSeconds(1);

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await HttpClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // Ignora exceções durante verificação de saúde
            }

            await Task.Delay(delay);
        }

        throw new TimeoutException("Aplicação não inicializou dentro do tempo limite esperado");
    }

    /// <summary>
    /// Garante que o schema do banco de dados está configurado
    /// </summary>
    protected virtual async Task EnsureDatabaseSchemaAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        try
        {
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao configurar schema do banco de dados para teste", ex);
        }
    }

    /// <summary>
    /// Executa operação com contexto do banco de dados
    /// </summary>
    protected async Task WithDbContextAsync(Func<UsersDbContext, Task> operation)
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await operation(context);
    }

    /// <summary>
    /// Executa operação com contexto do banco de dados e retorna resultado
    /// </summary>
    protected async Task<T> WithDbContextAsync<T>(Func<UsersDbContext, Task<T>> operation)
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        return await operation(context);
    }

    /// <summary>
    /// Cria opções de DbContext para uso direto
    /// </summary>
    protected DbContextOptions<TContext> CreateDbContextOptions<TContext>()
        where TContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseNpgsql(_postgresContainer!.GetConnectionString());
        return optionsBuilder.Options;
    }

    /// <summary>
    /// Helper para enviar requisições POST com serialização padrão
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        return await HttpClient.PostAsJsonAsync(requestUri, value, JsonOptions);
    }

    /// <summary>
    /// Helper para enviar requisições PUT com serialização padrão
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
    {
        return await HttpClient.PutAsJsonAsync(requestUri, value, JsonOptions);
    }

    /// <summary>
    /// Helper para deserializar respostas usando serialização padrão
    /// </summary>
    protected static async Task<T?> ReadFromJsonAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    /// <summary>
    /// Configura autenticação como usuário administrador
    /// </summary>
    protected static void AuthenticateAsAdmin()
    {
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();
    }

    /// <summary>
    /// Configura autenticação como usuário regular
    /// </summary>
    protected static void AuthenticateAsRegularUser(Guid? userId = null)
    {
        var userIdStr = (userId ?? Guid.NewGuid()).ToString();
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser(userIdStr, "testuser", "test@user.com");
    }

    /// <summary>
    /// Remove configuração de autenticação (usuário não autenticado)
    /// </summary>
    protected static void ClearAuthentication()
    {
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(Uri requestUri, T value)
    {
        throw new NotImplementedException();
    }

    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(Uri requestUri, T value)
    {
        throw new NotImplementedException();
    }
}
