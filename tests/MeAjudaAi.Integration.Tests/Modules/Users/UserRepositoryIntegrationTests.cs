using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Users;

/// <summary>
/// Integration tests for UserRepository with real database (TestContainers).
/// Tests actual persistence logic, EF mappings, and database constraints.
/// </summary>
public class UserRepositoryIntegrationTests : BaseApiTest
{
    private readonly Faker _faker = new("pt_BR");

    /// <summary>
    /// Adds a valid User via repository and verifies the user is persisted and retrievable by Id.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithValidUser_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = CreateValidUser();

        // Act - AddAsync auto-saves internally
        await repository.AddAsync(user);

        // Assert
        var retrieved = await repository.GetByIdAsync(user.Id);
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
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = CreateValidUser();
        await repository.AddAsync(user);

        // Act
        var result = await repository.GetByEmailAsync(user.Email);

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
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = CreateValidUser();
        await repository.AddAsync(user);

        // Act
        var result = await repository.GetByUsernameAsync(user.Username);

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
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user1 = CreateValidUser();
        var user2 = CreateValidUser();
        var user3 = CreateValidUser();
        await repository.AddAsync(user1);
        await repository.AddAsync(user2);
        await repository.AddAsync(user3);

        // Act
        var result = await repository.GetUsersByIdsAsync(new[] { user1.Id, user3.Id });

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
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        for (int i = 0; i < 15; i++)
        {
            await repository.AddAsync(CreateValidUser());
        }

        // Act
        var (users, totalCount) = await repository.GetPagedAsync(pageNumber: 1, pageSize: 10);

        // Assert
        users.Should().HaveCount(10);
        totalCount.Should().BeGreaterThanOrEqualTo(15);
    }

    /// <summary>
    /// Updates a user's profile and verifies the changes are persisted to the database.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithModifiedUser_ShouldPersistChanges()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = CreateValidUser();
        await repository.AddAsync(user);

        // Act - UpdateProfile modifies FirstName and LastName
        string newFirstName = _faker.Name.FirstName();
        string newLastName = _faker.Name.LastName();
        user.UpdateProfile(newFirstName, newLastName);
        await repository.UpdateAsync(user);

        // Assert
        var updated = await repository.GetByIdAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be(newFirstName);
        updated.LastName.Should().Be(newLastName);
    }

    private User CreateValidUser()
    {
        var username = new Username(_faker.Internet.UserName());
        var email = new Email(_faker.Internet.Email());
        var firstName = _faker.Name.FirstName();
        var lastName = _faker.Name.LastName();
        var keycloakId = UuidGenerator.NewId().ToString();

        return new User(username, email, firstName, lastName, keycloakId);
    }
}
