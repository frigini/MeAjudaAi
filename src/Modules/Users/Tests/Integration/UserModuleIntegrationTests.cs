using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;

namespace MeAjudaAi.Modules.Users.Tests.Integration;

/// <summary>
/// Exemplo de teste de integração usando containers compartilhados para melhor performance
/// </summary>
[Collection("UsersIntegrationTests")]
public class UserModuleIntegrationTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task CreateUser_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = CreateScope();
        var userDomainService = GetScopedService<IUserDomainService>(scope);
        var uow = GetScopedService<IUnitOfWork>(scope);
        var userQueries = GetScopedService<IUserQueries>(scope);
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

        createResult.IsSuccess.Should().BeTrue();

        var createdUser = createResult.Value;
        uow.GetRepository<User, UserId>().Add(createdUser);
        await uow.SaveChangesAsync();

        // Assert
        var retrievedUser = await userQueries.GetByIdAsync(createdUser.Id);
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Username.Value.Should().Be(username.Value);
        retrievedUser.Email.Value.Should().Be(email.Value);
        retrievedUser.FirstName.Should().Be(firstName);
        retrievedUser.LastName.Should().Be(lastName);

        messageBus.Should().NotBeNull();
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
        authResult.IsSuccess.Should().BeTrue();
        authResult.Value.Should().NotBeNull();
        authResult.Value!.AccessToken.Should().NotBeNull();
        authResult.Value.Roles.Should().Contain("customer");
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
        authResult.IsFailure.Should().BeTrue();
        authResult.Error.Should().NotBeNull();
        authResult.Error!.Message.Should().Be("Invalid credentials");
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
        validationResult.IsSuccess.Should().BeTrue();
        validationResult.Value.UserId.Should().NotBeNull();
        validationResult.Value.Roles.Should().Contain("customer");
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
        syncResult.IsSuccess.Should().BeTrue();
    }
}



