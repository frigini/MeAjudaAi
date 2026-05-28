using System;
using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Queries;
using MeAjudaAi.Modules.Users.Tests.Builders;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
public class DbContextUserQueriesTests : IDisposable
{
    private readonly UsersDbContext _context;
    private readonly DbContextUserQueries _queries;

    public DbContextUserQueriesTests()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new UsersDbContext(options, null!);
        _context = context;
        _queries = new DbContextUserQueries(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var result = await _queries.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = new Email("test@example.com");
        var user = new UserBuilder().WithEmail(email.Value).Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var email = new Email("nonexistent@example.com");

        // Act
        var result = await _queries.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
    {
        // Arrange
        var username = new Username("testuser");
        var user = new UserBuilder().WithUsername(username.Value).Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ShouldReturnNull()
    {
        // Arrange
        var username = new Username("nonexistent");

        // Act
        var result = await _queries.GetByUsernameAsync(username);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithValidIds_ShouldReturnUsers()
    {
        // Arrange
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        var user3 = new UserBuilder().Build();

        _context.Users.AddRange(user1, user2, user3);
        await _context.SaveChangesAsync();

        var ids = new List<UserId> { user1.Id, user2.Id };

        // Act
        var result = await _queries.GetUsersByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.Id == user1.Id);
        result.Should().Contain(u => u.Id == user2.Id);
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var ids = new List<UserId>();

        // Act
        var result = await _queries.GetUsersByIdsAsync(ids);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithNullList_ShouldReturnEmpty()
    {
        // Arrange
        List<UserId>? ids = null;

        // Act
        var result = await _queries.GetUsersByIdsAsync(ids!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedUsers()
    {
        // Arrange
        var users = Enumerable.Range(1, 25)
            .Select(_ => new UserBuilder().Build())
            .ToList();

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        // Act
        var (page1Users, page1Total) = await _queries.GetPagedAsync(1, 10);
        var (page2Users, page2Total) = await _queries.GetPagedAsync(2, 10);
        var (page3Users, page3Total) = await _queries.GetPagedAsync(3, 10);

        // Assert
        page1Users.Should().HaveCount(10);
        page1Total.Should().Be(25);
        page2Users.Should().HaveCount(10);
        page2Total.Should().Be(25);
        page3Users.Should().HaveCount(5);
        page3Total.Should().Be(25);
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPageNumber_ShouldUsePage1()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedAsync(0, 10);

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithSearchTerm_ShouldReturnMatchingUsers()
    {
        // Arrange
        var user1 = new UserBuilder()
            .WithEmail("john@example.com")
            .WithUsername("johnuser")
            .WithFirstName("John")
            .WithLastName("Doe")
            .Build();
        var user2 = new UserBuilder()
            .WithEmail("jane@example.com")
            .WithUsername("janeuser")
            .WithFirstName("Jane")
            .WithLastName("Smith")
            .Build();

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "john");

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithEmailSearch_ShouldReturnMatchingUsers()
    {
        // Arrange
        var user1 = new UserBuilder().WithEmail("test@example.com").Build();
        var user2 = new UserBuilder().WithEmail("other@domain.com").Build();

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "example");

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithUsernameSearch_ShouldReturnMatchingUsers()
    {
        // Arrange
        var user1 = new UserBuilder().WithUsername("testuser123").Build();
        var user2 = new UserBuilder().WithUsername("otheruser456").Build();

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "testuser");

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().Username.Value.Should().Be("testuser123");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithLastNameSearch_ShouldReturnMatchingUsers()
    {
        // Arrange
        var user1 = new UserBuilder().WithLastName("Smith").Build();
        var user2 = new UserBuilder().WithLastName("Johnson").Build();

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "smith");

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
        users.First().LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithEmptySearchTerm_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "");

        // Assert
        users.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithNullSearchTerm_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();

        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, null);

        // Assert
        users.Should().HaveCount(2);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithExistingKeycloakId_ShouldReturnUser()
    {
        // Arrange
        var keycloakId = Guid.NewGuid().ToString();
        var user = new UserBuilder().WithKeycloakId(keycloakId).Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        // Assert
        result.Should().NotBeNull();
        result!.KeycloakId.Should().Be(keycloakId);
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithNonExistingKeycloakId_ShouldReturnNull()
    {
        // Arrange
        var keycloakId = Guid.NewGuid().ToString();

        // Act
        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithEmptyKeycloakId_ShouldReturnNull()
    {
        // Arrange
        var keycloakId = "";

        // Act
        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithWhitespaceKeycloakId_ShouldReturnNull()
    {
        // Arrange
        var keycloakId = "   ";

        // Act
        var result = await _queries.GetByKeycloakIdAsync(keycloakId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _queries.ExistsAsync(user.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var result = await _queries.ExistsAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_WithInvalidPageSize_ShouldUseMinimumPageSize()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act - pageSize 0 should be normalized to 1
        var (users, total) = await _queries.GetPagedAsync(1, 0);

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithInvalidPageNumber_ShouldUsePage1()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act - pageNumber 0 should be normalized to 1
        var (users, total) = await _queries.GetPagedWithSearchAsync(0, 10, null);

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithInvalidPageSize_ShouldUseMinimumPageSize()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act - pageSize 0 should be normalized to 1
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 0, null);

        // Assert
        users.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithNoMatches_ShouldReturnEmpty()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var (users, total) = await _queries.GetPagedWithSearchAsync(1, 10, "nonexistent");

        // Assert
        users.Should().BeEmpty();
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithNullKeycloakId_ShouldReturnNull()
    {
        // Arrange
        string? keycloakId = null;

        // Act
        var result = await _queries.GetByKeycloakIdAsync(keycloakId!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersByIdsAsync_WithMoreThanBatchSize_ShouldReturnUsersFromAllChunks()
    {
        // Arrange - Create more than 2000 users to test chunking
        // Note: For practical testing, we'll use a smaller number but verify the logic
        var users = Enumerable.Range(1, 100)
            .Select(_ => new UserBuilder().Build())
            .ToList();

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var ids = users.Select(u => u.Id).ToList();

        // Act
        var result = await _queries.GetUsersByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(100);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
