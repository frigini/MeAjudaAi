using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Tests.Infrastructure;

namespace MeAjudaAi.Modules.Users.Tests.Integration;

/// <summary>
/// Exemplo de teste de integração usando containers compartilhados para melhor performance
/// </summary>
[Collection("UsersIntegrationTests")]
public class UserModuleIntegrationTests : UsersIntegrationTestBase
{
    // Remove override to use default SharedTestContainers configuration
    // protected override TestInfrastructureOptions GetTestOptions() - using inherited default
    
    [Fact]
    public async Task CreateUser_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = CreateScope();
        var userDomainService = GetScopedService<IUserDomainService>(scope);
        var userRepository = GetScopedService<IUserRepository>(scope);
        var dbContext = GetScopedService<UsersDbContext>(scope);
        var messageBus = GetScopedService<IMessageBus>(scope);
        
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
        
        // Assert - Verificar se o message bus está configurado (mock)
        Assert.NotNull(messageBus);
        // Note: No teste real, eventos de domínio são publicados automaticamente pelo EF
    }
    
    [Fact]
    public async Task AuthenticateUser_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        using var scope = CreateScope();
        var authService = GetScopedService<IAuthenticationDomainService>(scope);
        
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
        using var scope = CreateScope();
        var authService = GetScopedService<IAuthenticationDomainService>(scope);
        
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
        using var scope = CreateScope();
        var authService = GetScopedService<IAuthenticationDomainService>(scope);
        
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
        using var scope = CreateScope();
        var userDomainService = GetScopedService<IUserDomainService>(scope);
        var userId = new UserId(Guid.NewGuid());
        
        // Act
        var syncResult = await userDomainService.SyncUserWithKeycloakAsync(userId);
        
        // Assert
        Assert.True(syncResult.IsSuccess);
    }
}