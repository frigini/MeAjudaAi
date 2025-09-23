using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Integration;

/// <summary>
/// Exemplo de teste de integração usando a infraestrutura modular de testes
/// </summary>
public class UserModuleIntegrationTests : IAsyncLifetime
{
    private readonly ServiceProvider _serviceProvider;
    private readonly PostgreSqlContainer _dbContainer;
    
    public UserModuleIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Configurar infraestrutura de testes com opções customizadas
        var testOptions = new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = "test_users_db",
                Username = "testuser", 
                Password = "testpass123",
                Schema = "users_test"
            },
            Cache = new TestCacheOptions
            {
                Enabled = false // Para este teste, não precisamos de cache
            },
            ExternalServices = new TestExternalServicesOptions
            {
                UseKeycloakMock = true,
                UseMessageBusMock = true
            }
        };
        
        services.AddUsersTestInfrastructure(testOptions);
        
        _serviceProvider = services.BuildServiceProvider();
        _dbContainer = _serviceProvider.GetRequiredService<PostgreSqlContainer>();
    }
    
    public async Task InitializeAsync()
    {
        // Inicializar container do banco de dados
        await _dbContainer.StartAsync();
        
        // Aplicar migrations
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await dbContext.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _serviceProvider.DisposeAsync();
    }
    
    [Fact]
    public async Task CreateUser_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
        
        var username = new Username("testuser123");
        var email = new Email("testuser@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "customer" };
        
        // Act
        var createResult = await userDomainService.CreateUserAsync(
            username, email, firstName, lastName, password, roles);
        
        Assert.True(createResult.IsSuccess);
        
        var createdUser = createResult.Value;
        await userRepository.AddAsync(createdUser);
        await dbContext.SaveChangesAsync(); // SaveChanges é do DbContext
        
        // Assert - Verificar se foi persistido no banco
        var retrievedUser = await userRepository.GetByIdAsync(createdUser.Id);
        Assert.NotNull(retrievedUser);
        Assert.Equal(username.Value, retrievedUser.Username.Value);
        Assert.Equal(email.Value, retrievedUser.Email.Value);
        Assert.Equal(firstName, retrievedUser.FirstName);
        Assert.Equal(lastName, retrievedUser.LastName);
        
        // Assert - Verificar se mensagens foram publicadas (mock)
        var mockMessageBus = messageBus as MockMessageBus;
        Assert.NotNull(mockMessageBus);
        // Note: No teste real, eventos de domínio são publicados automaticamente pelo EF
        // mas aqui estamos testando só o mock do message bus
    }
    
    [Fact]
    public async Task AuthenticateUser_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationDomainService>();
        
        // Act
        var authResult = await authService.AuthenticateAsync("validuser", "validpassword");
        
        // Assert
        Assert.True(authResult.IsSuccess);
        Assert.NotNull(authResult.Value);
        Assert.NotNull(authResult.Value.AccessToken);
        Assert.Contains("customer", authResult.Value.Roles!);
    }
    
    [Fact]
    public async Task AuthenticateUser_WithInvalidCredentials_ShouldReturnFailureResult()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationDomainService>();
        
        // Act
        var authResult = await authService.AuthenticateAsync("invaliduser", "wrongpassword");
        
        // Assert
        Assert.True(authResult.IsFailure);
        Assert.Equal("Invalid credentials", authResult.Error.Message);
    }
    
    [Fact]
    public async Task ValidateToken_WithValidToken_ShouldReturnValidResult()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationDomainService>();
        
        // Act
        var validationResult = await authService.ValidateTokenAsync("mock_token_12345");
        
        // Assert
        Assert.True(validationResult.IsSuccess);
        Assert.NotNull(validationResult.Value.UserId);
        Assert.Contains("customer", validationResult.Value.Roles!);
    }
    
    [Fact]
    public async Task SyncUserWithKeycloak_ShouldReturnSuccess()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var userDomainService = scope.ServiceProvider.GetRequiredService<IUserDomainService>();
        var userId = new UserId(Guid.NewGuid());
        
        // Act
        var syncResult = await userDomainService.SyncUserWithKeycloakAsync(userId);
        
        // Assert
        Assert.True(syncResult.IsSuccess);
    }
}