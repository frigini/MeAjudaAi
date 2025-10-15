using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Tests.Integration;

/// <summary>
/// Testes de integração para GetUserByUsernameQuery
/// </summary>
[Collection("UsersIntegrationTests")]
public class GetUserByUsernameQueryIntegrationTests : UsersIntegrationTestBase
{
    [Fact]
    public async Task GetUserByUsername_WithExistingUser_ShouldReturnUserSuccessfully()
    {
        // Arrange
        using var scope = CreateScope();
        var userDomainService = GetScopedService<IUserDomainService>(scope);
        var userRepository = GetScopedService<IUserRepository>(scope);
        var dbContext = GetScopedService<UsersDbContext>(scope);
        var queryHandler = GetScopedService<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>>(scope);

        // Cria um usuário de teste primeiro
        var username = new Username($"test{Guid.NewGuid().ToString()[..6]}");
        var email = new Email($"test{Guid.NewGuid().ToString()[..6]}@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "customer" };

        var createResult = await userDomainService.CreateUserAsync(
            username, email, firstName, lastName, password, roles);

        Assert.True(createResult.IsSuccess);

        var createdUser = createResult.Value;
        await userRepository.AddAsync(createdUser);
        await dbContext.SaveChangesAsync();

        // Act - Consulta o usuário pelo nome de usuário
        var query = new GetUserByUsernameQuery(username.Value);
        var queryResult = await queryHandler.HandleAsync(query);

        // Assert
        Assert.True(queryResult.IsSuccess);
        Assert.NotNull(queryResult.Value);
        Assert.Equal(username.Value, queryResult.Value.Username);
        Assert.Equal(email.Value, queryResult.Value.Email);
        Assert.Equal(firstName, queryResult.Value.FirstName);
        Assert.Equal(lastName, queryResult.Value.LastName);
    }

    [Fact]
    public async Task GetUserByUsername_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        using var scope = CreateScope();
        var queryHandler = GetScopedService<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>>(scope);

        var nonExistentUsername = $"fake{Guid.NewGuid().ToString()[..6]}";

        // Act
        var query = new GetUserByUsernameQuery(nonExistentUsername);
        var queryResult = await queryHandler.HandleAsync(query);

        // Assert
        Assert.False(queryResult.IsSuccess);
        Assert.NotNull(queryResult.Error);
        Assert.Contains("User not found", queryResult.Error.Message);
    }

    [Fact]
    public async Task UsernameExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        using var scope = CreateScope();
        var userDomainService = GetScopedService<IUserDomainService>(scope);
        var userRepository = GetScopedService<IUserRepository>(scope);
        var dbContext = GetScopedService<UsersDbContext>(scope);
        var usersModuleApi = GetScopedService<IUsersModuleApi>(scope);

        // Cria um usuário de teste primeiro
        var username = new Username($"test{Guid.NewGuid().ToString()[..6]}");
        var email = new Email($"test{Guid.NewGuid().ToString()[..6]}@example.com");
        var firstName = "Test";
        var lastName = "User";
        var password = "SecurePassword123!";
        var roles = new[] { "customer" };

        var createResult = await userDomainService.CreateUserAsync(
            username, email, firstName, lastName, password, roles);

        Assert.True(createResult.IsSuccess);

        var createdUser = createResult.Value;
        await userRepository.AddAsync(createdUser);
        await dbContext.SaveChangesAsync();

        // Act - Verifica se o nome de usuário existe
        var existsResult = await usersModuleApi.UsernameExistsAsync(username.Value);

        // Assert
        Assert.True(existsResult.IsSuccess);
        Assert.True(existsResult.Value);
    }

    [Fact]
    public async Task UsernameExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        using var scope = CreateScope();
        var usersModuleApi = GetScopedService<IUsersModuleApi>(scope);

        var nonExistentUsername = $"fake{Guid.NewGuid().ToString()[..6]}";

        // Act
        var existsResult = await usersModuleApi.UsernameExistsAsync(nonExistentUsername);

        // Assert
        Assert.True(existsResult.IsSuccess);
        Assert.False(existsResult.Value);
    }
}