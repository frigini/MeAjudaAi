using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Base;
using MeAjudaAi.Integration.Tests.Auth;
using MeAjudaAi.Shared.Tests.Mocks.Messaging;
using MeAjudaAi.ApiService.Handlers;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Modules.Users.Domain.Services.Models;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Classe base para testes de integração com API usando TestContainers PostgreSQL real
/// </summary>
public abstract class ApiTestBase : DatabaseTestBase, IAsyncLifetime
{
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    async Task IAsyncLifetime.InitializeAsync()
    {
        // Inicializa o TestContainer PostgreSQL primeiro
        await base.InitializeAsync();

        // Define ambiente Testing ANTES de criar a Factory
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // Cria factory da aplicação com configuração de teste
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Adiciona configuração de teste que sobrescreve connection strings
                    var testConfig = new Dictionary<string, string>
                    {
                        ["ConnectionStrings:DefaultConnection"] = ConnectionString, // ✅ Nova connection string padrão
                        ["ConnectionStrings:Users"] = ConnectionString,
                        ["ConnectionStrings:meajudaai-db"] = ConnectionString,
                        ["Postgres:ConnectionString"] = ConnectionString,
                        ["Messaging:Enabled"] = "false",
                        ["Caching:Enabled"] = "false"
                    };
                    
                    config.AddInMemoryCollection(testConfig!);
                });
                
                builder.ConfigureServices(services =>
                {
                    // Remove qualquer DbContext configurado e adiciona o nosso com TestContainer
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Configura DbContext com connection string do TestContainer
                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(ConnectionString, npgsqlOptions =>
                        {
                            npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
                        })
                        .UseSnakeCaseNamingConvention()
                        // Configurações consistentes para evitar problemas com compiled queries
                        .EnableServiceProviderCaching()
                        .EnableSensitiveDataLogging(false);
                        
                        // Suprime warning sobre mudanças pendentes no modelo durante testes
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });

                    // Configura mocks de messaging (FASE 2.3)
                    services.AddMessagingMocks();

                    // Remove e substitui IKeycloakService por mock para testes
                    var keycloakDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IKeycloakService));
                    if (keycloakDescriptor != null)
                        services.Remove(keycloakDescriptor);

                    // Adiciona mock do IKeycloakService para testes
                    services.AddSingleton<IKeycloakService>(provider => new MockKeycloakService());

                    // Remove a autenticação JWT configurada em produção
                    var authDescriptors = services.Where(d => d.ServiceType == typeof(IAuthenticationSchemeProvider)).ToList();
                    foreach (var authDescriptor in authDescriptors)
                    {
                        services.Remove(authDescriptor);
                    }

                    // Configura autenticação de teste como default
                    services.AddAuthentication(defaultScheme: FakeAuthenticationHandler.SchemeName)
                        .AddScheme<AuthenticationSchemeOptions, FakeAuthenticationHandler>(
                            FakeAuthenticationHandler.SchemeName, 
                            options => { });

                    // Reconfigura authorization para usar as mesmas políticas mas com fake authentication
                    services.AddAuthorizationBuilder()
                        .AddPolicy("AdminOnly", policy =>
                            policy.RequireRole("admin"))
                        .AddPolicy("SuperAdminOnly", policy =>
                            policy.RequireRole("super-admin"))
                        .AddPolicy("UserManagement", policy =>
                            policy.RequireRole("admin"))
                        .AddPolicy("ServiceProviderAccess", policy =>
                            policy.RequireRole("service-provider", "admin"))
                        .AddPolicy("CustomerAccess", policy =>
                            policy.RequireRole("customer", "admin"))
                        .AddPolicy("SelfOrAdmin", policy =>
                            policy.AddRequirements(new SelfOrAdminRequirement()));

                    // Register authorization handlers
                    services.AddScoped<IAuthorizationHandler, SelfOrAdminHandler>();
                });
            });

        Client = Factory.CreateClient();

        // Aplica migrações e prepara banco
        await EnsureDatabaseAsync();

        // Inicializa Respawner após as migrações
        await InitializeRespawnerAsync();
    }

    /// <summary>
    /// Garante que o banco está criado e com migrações aplicadas
    /// </summary>
    private async Task EnsureDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        
        // Aplica migrações se necessário
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Limpa dados entre testes mantendo estrutura
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        await ResetDatabaseAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        await base.DisposeAsync();
    }
}

/// <summary>
/// Mock do IKeycloakService para testes de integração
/// </summary>
public class MockKeycloakService : IKeycloakService
{
    public Task<Result<string>> CreateUserAsync(string username, string email, string firstName, string lastName, 
        string password, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        // Simula criação bem-sucedida retornando um ID de usuário fictício
        var keycloakId = Guid.NewGuid().ToString();
        return Task.FromResult(Result<string>.Success(keycloakId));
    }

    public Task<Result<AuthenticationResult>> AuthenticateAsync(string usernameOrEmail, string password, 
        CancellationToken cancellationToken = default)
    {
        // Para testes, sempre retorna autenticação bem-sucedida
        var authResult = new AuthenticationResult
        {
            AccessToken = "fake-access-token",
            RefreshToken = "fake-refresh-token",
            UserId = Guid.NewGuid()
        };
        return Task.FromResult(Result<AuthenticationResult>.Success(authResult));
    }

    public Task<Result<TokenValidationResult>> ValidateTokenAsync(string token, 
        CancellationToken cancellationToken = default)
    {
        // Para testes, sempre retorna token válido
        var validationResult = new TokenValidationResult
        {
            UserId = Guid.NewGuid()
        };
        return Task.FromResult(Result<TokenValidationResult>.Success(validationResult));
    }

    public Task<Result> DeactivateUserAsync(string keycloakId, CancellationToken cancellationToken = default)
    {
        // Para testes, sempre retorna desativação bem-sucedida
        return Task.FromResult(Result.Success());
    }
}