using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Integration tests for UserRepository with real database (TestContainers).
/// Tests actual persistence logic, EF mappings, and database constraints.
/// </summary>
public class UserRepositoryIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Users;

    private readonly Faker _faker = new("pt_BR");

    /// <summary>
    /// Adds a valid User via repository and verifies the user is persisted and retrievable by Id.
    /// </summary>
    [Fact]
    public async Task Add_WithValidUser_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var userQueries = scope.ServiceProvider.GetRequiredService<IUserQueries>();
        var repository = uow.GetRepository<User, UserId>();
        var user = CreateValidUser();

        // Act
        repository.Add(user);
        await uow.SaveChangesAsync();

        // Assert
        var retrieved = await userQueries.GetByIdAsync(user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
        retrieved.Email.Value.Should().Be(user.Email.Value);
        retrieved.FirstName.Should().Be(user.FirstName);
        retrieved.LastName.Should().Be(user.LastName);
    }

    /// <summary>
    /// Retrieves a user by email and verifies the correct user is returned.
    /// </summary>
    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var userQueries = scope.ServiceProvider.GetRequiredService<IUserQueries>();
        var user = CreateValidUser();
        uow.GetRepository<User, UserId>().Add(user);
        await uow.SaveChangesAsync();

        // Act
        var result = await userQueries.GetByEmailAsync(user.Email);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Value.Should().Be(user.Email.Value);
    }

    /// <summary>
    /// Retrieves a user by username and verifies the correct user is returned.
    /// </summary>
    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var userQueries = scope.ServiceProvider.GetRequiredService<IUserQueries>();
        var user = CreateValidUser();
        uow.GetRepository<User, UserId>().Add(user);
        await uow.SaveChangesAsync();

        // Act
        var result = await userQueries.GetByUsernameAsync(user.Username);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Value.Should().Be(user.Username.Value);
    }

    /// <summary>
    /// Retrieves multiple users by their IDs and verifies all matching users are returned.
    /// </summary>
    [Fact]
    public async Task GetUsersByIdsAsync_WithMultipleIds_ShouldReturnMatchingUsers()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var userQueries = scope.ServiceProvider.GetRequiredService<IUserQueries>();
        var repository = uow.GetRepository<User, UserId>();
        var user1 = CreateValidUser();
        var user2 = CreateValidUser();
        var user3 = CreateValidUser();
        repository.Add(user1);
        repository.Add(user2);
        repository.Add(user3);
        await uow.SaveChangesAsync();

        // Act
        var result = await userQueries.GetUsersByIdsAsync(new[] { user1.Id, user3.Id });

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == user1.Id);
        result.Should().Contain(u => u.Id == user3.Id);
    }

    /// <summary>
    /// Retrieves a paged list of users and verifies the page size is respected.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var userQueries = scope.ServiceProvider.GetRequiredService<IUserQueries>();
        var repository = uow.GetRepository<User, UserId>();
        for (int i = 0; i < 15; i++)
        {
            repository.Add(CreateValidUser());
        }
        await uow.SaveChangesAsync();

        // Act
        var (users, totalCount) = await userQueries.GetPagedAsync(pageNumber: 1, pageSize: 10);

        // Assert
        users.Should().HaveCount(10);
        totalCount.Should().BeGreaterThanOrEqualTo(15);
    }

    private User CreateValidUser()
    {
        var suffix = Guid.NewGuid().ToString("n")[..6];
        var fakerUser = _faker.Internet.UserName();
        if (fakerUser.Length > 20) fakerUser = fakerUser[..20];
        
        var username = new Username(fakerUser + "_" + suffix);
        var email = new Email(suffix + "_" + _faker.Internet.Email());
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var keycloakId = UuidGenerator.NewId().ToString();

        return User.Create(username, email, firstName, lastName, keycloakId).Value!;
    }
}
