using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Infrastructure.Identity.Keycloak;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Domain.Services.Models;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Users.Tests.Infrastructure;

/// <summary>
/// Extensões para configurar infraestrutura de testes específica do módulo Users
/// </summary>
public static class TestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona toda a infraestrutura de testes necessária para o módulo Users
    /// </summary>
    public static IServiceCollection AddUsersTestInfrastructure(
        this IServiceCollection services, 
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();
        
        services.AddSingleton(options);
        
        // Configurar banco de dados de teste
        services.AddTestDatabase(options.Database);
        
        // Configurar cache de teste (se necessário)
        if (options.Cache.Enabled)
        {
            services.AddTestCache(options.Cache);
        }
        
        // Configurar mocks de serviços externos
        services.AddTestExternalServices(options.ExternalServices);
        
        // Adicionar repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
    
    private static IServiceCollection AddTestDatabase(
        this IServiceCollection services, 
        TestDatabaseOptions options)
    {
        // Configurar TestContainer para PostgreSQL
        services.AddSingleton<PostgreSqlContainer>(provider =>
        {
            var container = new PostgreSqlBuilder()
                .WithImage(options.PostgresImage)
                .WithDatabase(options.DatabaseName)
                .WithUsername(options.Username)
                .WithPassword(options.Password)
                .WithCleanUp(true)
                .Build();
                
            return container;
        });
        
        // Configurar DbContext com TestContainer
        services.AddDbContext<UsersDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();
            
            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Schema);
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
    
    private static IServiceCollection AddTestCache(
        this IServiceCollection services, 
        TestCacheOptions options)
    {
        // Para testes simples, usar cache em memória ao invés de Redis
        services.AddMemoryCache();
        
        return services;
    }
    
    private static IServiceCollection AddTestExternalServices(
        this IServiceCollection services, 
        TestExternalServicesOptions options)
    {
        if (options.UseKeycloakMock)
        {
            // Substituir serviços reais por mocks
            services.Replace(ServiceDescriptor.Scoped<IKeycloakService, MockKeycloakService>());
            services.Replace(ServiceDescriptor.Scoped<IUserDomainService, MockUserDomainService>());
            services.Replace(ServiceDescriptor.Scoped<IAuthenticationDomainService, MockAuthenticationDomainService>());
        }
        
        if (options.UseMessageBusMock)
        {
            // Usar mock do message bus
            services.Replace(ServiceDescriptor.Scoped<IMessageBus, MockMessageBus>());
        }
        
        return services;
    }
}

/// <summary>
/// Implementações mock específicas para testes do módulo Users
/// </summary>
internal class MockUserDomainService : IUserDomainService
{
    public Task<Result<User>> CreateUserAsync(
        Username username, 
        Email email, 
        string firstName, 
        string lastName, 
        string password, 
        IEnumerable<string> roles, 
        CancellationToken cancellationToken = default)
    {
        // Para testes, criar usuário mock
        var user = new User(username, email, firstName, lastName, $"keycloak_{Guid.NewGuid()}");
        return Task.FromResult(Result<User>.Success(user));
    }
    
    public Task<Result> SyncUserWithKeycloakAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        // Para testes, simular sincronização bem-sucedida
        return Task.FromResult(Result.Success());
    }
}

internal class MockAuthenticationDomainService : IAuthenticationDomainService
{
    public Task<Result<AuthenticationResult>> AuthenticateAsync(
        string usernameOrEmail, 
        string password, 
        CancellationToken cancellationToken = default)
    {
        // Para testes, validar apenas credenciais específicas
        if (usernameOrEmail == "validuser" && password == "validpassword")
        {
            var result = new AuthenticationResult(
                UserId: Guid.NewGuid(),
                AccessToken: $"mock_token_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                RefreshToken: $"mock_refresh_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
                ExpiresAt: DateTime.UtcNow.AddHours(1),
                Roles: new[] { "customer" }
            );
            return Task.FromResult(Result<AuthenticationResult>.Success(result));
        }
        
        return Task.FromResult(Result<AuthenticationResult>.Failure("Invalid credentials"));
    }
    
    public Task<Result<TokenValidationResult>> ValidateTokenAsync(
        string token, 
        CancellationToken cancellationToken = default)
    {
        // Para testes, validar tokens que começam com "mock_token_"
        if (token.StartsWith("mock_token_"))
        {
            var result = new TokenValidationResult(
                UserId: Guid.NewGuid(),
                Roles: new[] { "customer" },
                Claims: new Dictionary<string, object> { ["sub"] = Guid.NewGuid().ToString() }
            );
            return Task.FromResult(Result<TokenValidationResult>.Success(result));
        }
        
        var invalidResult = new TokenValidationResult(
            UserId: null,
            Roles: Array.Empty<string>(),
            Claims: new Dictionary<string, object>()
        );
        return Task.FromResult(Result<TokenValidationResult>.Success(invalidResult));
    }
}

internal class MockMessageBus : IMessageBus
{
    private readonly List<object> _publishedMessages = new();
    
    public IReadOnlyList<object> PublishedMessages => _publishedMessages.AsReadOnly();
    
    public Task SendAsync<TMessage>(TMessage message, string? queueName = null, CancellationToken cancellationToken = default)
    {
        _publishedMessages.Add(message!);
        return Task.CompletedTask;
    }
    
    public Task PublishAsync<TMessage>(TMessage @event, string? topicName = null, CancellationToken cancellationToken = default)
    {
        _publishedMessages.Add(@event!);
        return Task.CompletedTask;
    }
    
    public Task SubscribeAsync<TMessage>(Func<TMessage, CancellationToken, Task> handler, string? subscriptionName = null, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
    
    public void ClearMessages()
    {
        _publishedMessages.Clear();
    }
}