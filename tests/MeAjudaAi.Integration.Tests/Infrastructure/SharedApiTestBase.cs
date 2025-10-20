using System.Net.Http.Json;
using Bogus;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure.Mocks;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.Mocks.Messaging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Classe base genérica para testes de integração de API
/// Utiliza TestContainers para PostgreSQL com configuração otimizada para CI/CD
/// Suporte genérico a qualquer programa/módulo através de TProgram
/// </summary>
public abstract class SharedApiTestBase<TProgram> : IAsyncLifetime
    where TProgram : class
{
    private PostgreSqlContainer? _postgresContainer;
    private WebApplicationFactory<TProgram>? _factory;

    protected HttpClient HttpClient { get; private set; } = null!;
    protected HttpClient Client => HttpClient; // Alias para compatibilidade
    protected WebApplicationFactory<TProgram> Factory => _factory!;
    protected IServiceProvider Services => _factory!.Services;
    protected Faker Faker { get; } = new();

    /// <summary>
    /// Opções de serialização JSON padrão do sistema
    /// </summary>
    protected static System.Text.Json.JsonSerializerOptions JsonOptions => SerializationDefaults.Api;

    /// <summary>
    /// Configurações específicas do teste - DEVE usar connection string do container
    /// </summary>
    protected virtual Dictionary<string, string?> GetTestConfiguration()
    {
        return new Dictionary<string, string?>
        {
            {"ConnectionStrings:DefaultConnection", _postgresContainer?.GetConnectionString()},
            {"ConnectionStrings:meajudaai-db-local", _postgresContainer?.GetConnectionString()},
            {"ConnectionStrings:users-db", _postgresContainer?.GetConnectionString()},
            {"Postgres:ConnectionString", _postgresContainer?.GetConnectionString()},
            {"ASPNETCORE_ENVIRONMENT", "Testing"},
            {"INTEGRATION_TESTS", "true"}, // IMPORTANTE: Para usar FakeIntegrationAuthenticationHandler em vez de TestAuthenticationHandler
            {"Logging:LogLevel:Default", "Warning"},
            {"Logging:LogLevel:Microsoft", "Error"},
            {"Logging:LogLevel:Microsoft.AspNetCore", "Error"},
            {"Logging:LogLevel:Microsoft.EntityFrameworkCore", "Error"},
            // Desabilita serviços desnecessários
            {"Messaging:Enabled", "false"},
            {"Cache:Enabled", "false"},
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
    }

    public virtual async ValueTask InitializeAsync()
    {
        // CRUCIAL: Limpa configuração de autenticação ANTES de inicializar aplicação
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // Configura e inicia PostgreSQL
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();

        // Configura WebApplicationFactory seguindo padrão E2E
        _factory = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(GetTestConfiguration());

                    // CRITICAL: Define variável de ambiente para que EnvironmentSpecificExtensions use FakeIntegrationAuthenticationHandler
                    Environment.SetEnvironmentVariable("INTEGRATION_TESTS", "true");
                });

                builder.ConfigureServices((context, services) =>
                {
                    // Remove serviços hospedados problemáticos
                    var hostedServices = services
                        .Where(descriptor => descriptor.ServiceType == typeof(IHostedService))
                        .ToList();

                    foreach (var service in hostedServices)
                    {
                        services.Remove(service);
                    }

                    // CRUCIAL: Remove TODOS os registros relacionados ao DbContext antes de reconfigurar
                    var dbContextDescriptors = services.Where(s =>
                        s.ServiceType == typeof(UsersDbContext) ||
                        s.ServiceType == typeof(DbContextOptions<UsersDbContext>) ||
                        (s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                    ).ToList();

                    foreach (var desc in dbContextDescriptors)
                    {
                        services.Remove(desc);
                    }

                    // Agora registra com a connection string do container
                    var containerConnectionString = _postgresContainer.GetConnectionString();

                    // REGISTRAR IDomainEventProcessor PARA PROCESSAR DOMAIN EVENTS
                    services.AddScoped<IDomainEventProcessor, DomainEventProcessor>();

                    // REGISTRAR UsersDbContext COM IDomainEventProcessor para processar domain events

                    // Registra usando factory method que força o uso do construtor COM IDomainEventProcessor
                    services.AddScoped<UsersDbContext>(serviceProvider =>
                    {
                        var options = new DbContextOptionsBuilder<UsersDbContext>()
                            .UseNpgsql(containerConnectionString)
                            .EnableSensitiveDataLogging(false)
                            .LogTo(_ => { }, LogLevel.Error)
                            .Options;

                        var domainEventProcessor = serviceProvider.GetRequiredService<IDomainEventProcessor>();
                        return new UsersDbContext(options, domainEventProcessor);  // Usa o construtor runtime COM IDomainEventProcessor
                    });

                    // Também registra as DbContextOptions para injeção
                    services.AddSingleton<DbContextOptions<UsersDbContext>>(serviceProvider =>
                    {
                        return new DbContextOptionsBuilder<UsersDbContext>()
                            .UseNpgsql(containerConnectionString)
                            .EnableSensitiveDataLogging(false)
                            .LogTo(_ => { }, LogLevel.Error)
                            .Options;
                    });

                    // BRUTAL APPROACH: Remove TODA configuração de authentication/authorization e reconfigure do zero
                    var authServices = services.Where(s =>
                        s.ServiceType.Namespace?.Contains("Authentication") == true ||
                        s.ServiceType.Namespace?.Contains("Authorization") == true ||
                        (s.ImplementationType?.Name.Contains("AuthenticationHandler") == true) ||
                        s.ServiceType == typeof(IAuthenticationService) ||
                        s.ServiceType == typeof(IAuthenticationSchemeProvider) ||
                        s.ServiceType == typeof(IAuthenticationHandlerProvider)
                    ).ToList();

                    foreach (var service in authServices)
                    {
                        services.Remove(service);
                    }

                    // Reconfigura autenticação E autorização completamente do zero

                    // Primeiro adiciona autorização básica com políticas necessárias
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("SelfOrAdmin", policy =>
                            policy.AddRequirements(new MeAjudaAi.ApiService.Handlers.SelfOrAdminRequirement()));
                        options.AddPolicy("AdminOnly", policy =>
                            policy.RequireRole("admin", "super-admin"));
                        options.AddPolicy("SuperAdminOnly", policy =>
                            policy.RequireRole("super-admin"));
                        options.AddPolicy("UserManagement", policy =>
                            policy.RequireRole("admin", "super-admin"));
                        options.AddPolicy("ServiceProviderAccess", policy =>
                            policy.RequireRole("service-provider", "admin", "super-admin"));
                        options.AddPolicy("CustomerAccess", policy =>
                            policy.RequireRole("customer", "admin", "super-admin"));
                    });

                    // Registra o handler de autorização necessário
                    services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, MeAjudaAi.ApiService.Handlers.SelfOrAdminHandler>();

                    // Depois adiciona nossa autenticação configurável COM esquema padrão forçado
                    services.AddConfigurableTestAuthentication();

                    // FORÇA esquema padrão para nosso handler configurável
                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestConfigurable";
                        options.DefaultChallengeScheme = "TestConfigurable";
                        options.DefaultScheme = "TestConfigurable";
                    });

                    // FORÇA ambiente não-Testing temporariamente para que messaging seja adicionado
                    var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

                    try
                    {
                        // Adiciona shared services que incluem messaging
                        services.AddSharedServices(context.Configuration);
                    }
                    finally
                    {
                        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
                    }

                    // Adiciona mocks de messaging para sobrescrever implementações reais
                    services.AddMessagingMocks();

                    // FORÇA registros específicos de messaging que podem não estar sendo detectados pelo Scrutor
                    services.AddSingleton<MeAjudaAi.Shared.Tests.Mocks.Messaging.MockServiceBusMessageBus>();
                    services.AddSingleton<MeAjudaAi.Shared.Tests.Mocks.Messaging.MockRabbitMqMessageBus>();

                    // Event Handlers são registrados pelo próprio módulo Users via Extensions.AddEventHandlers()

                    // FORÇA Mock do cache para evitar conexões Redis nos testes
                    var cacheDescriptors = services.Where(s => s.ServiceType == typeof(Microsoft.Extensions.Caching.Distributed.IDistributedCache)).ToList();
                    foreach (var desc in cacheDescriptors)
                    {
                        services.Remove(desc);
                    }
                    services.AddMemoryCache();
                    services.AddSingleton<Microsoft.Extensions.Caching.Distributed.IDistributedCache, Microsoft.Extensions.Caching.Distributed.MemoryDistributedCache>();

                    // FORÇA MockKeycloakService para testes
                    var keycloakDescriptors = services.Where(s => s.ServiceType.Name.Contains("IKeycloakService")).ToList();
                    foreach (var desc in keycloakDescriptors)
                    {
                        services.Remove(desc);
                    }
                    services.AddScoped<MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak.IKeycloakService, MockKeycloakService>();

                    // Configura HostOptions para ignoreexceções
                    services.Configure<HostOptions>(options =>
                    {
                        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
                    });
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning); // Reduzido para menos verbosidade

                    // Apenas erros para logs desnecessários
                    logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Error);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
                    logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Error);
                    logging.AddFilter("MeAjudaAi.Shared.Tests.Auth", LogLevel.Error);
                });
            });

        HttpClient = _factory.CreateClient();

        // Aguarda inicialização
        await WaitForApplicationStartup();

        // Aplica migrações
        await EnsureDatabaseSchemaAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        HttpClient?.Dispose();
        _factory?.Dispose();

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
                // Ignora exceções durante verificação
            }

            await Task.Delay(delay);
        }

        throw new TimeoutException("Aplicação não inicializou dentro do tempo esperado");
    }

    /// <summary>
    /// Garante que o schema do banco está configurado
    /// </summary>
    protected virtual async Task EnsureDatabaseSchemaAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        try
        {
            // Para Integration tests, sempre recriar o banco do zero para evitar conflitos
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Falha ao configurar schema do banco para teste", ex);
        }
    }

    /// <summary>
    /// Reset do banco de dados - compatibilidade com testes existentes
    /// </summary>
    protected async Task ResetDatabaseAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        try
        {
            // Garante que o schema existe primeiro
            await context.Database.EnsureCreatedAsync();

            // Limpa todas as tabelas mantendo o schema
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE users.\"Users\" RESTART IDENTITY CASCADE");
        }
        catch (Exception ex)
        {
            // Se TRUNCATE falhar, tenta DROP + CREATE (mais agressivo mas funciona)
            Console.WriteLine($"[RESET-DB] TRUNCATE failed ({ex.Message}), trying DROP+CREATE");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
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
    /// Executa operação com contexto e retorna resultado
    /// </summary>
    protected async Task<T> WithDbContextAsync<T>(Func<UsersDbContext, Task<T>> operation)
    {
        using var scope = _factory!.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        return await operation(context);
    }

    /// <summary>
    /// Helper para POST com serialização padrão
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        return await HttpClient.PostAsJsonAsync(requestUri, value, JsonOptions);
    }

    /// <summary>
    /// Helper para PUT com serialização padrão
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

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(Uri requestUri, T value)
    {
        throw new NotImplementedException();
    }

    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(Uri requestUri, T value)
    {
        throw new NotImplementedException();
    }
}
