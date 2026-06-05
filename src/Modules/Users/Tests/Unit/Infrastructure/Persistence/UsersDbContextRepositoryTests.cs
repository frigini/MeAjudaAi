using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Persistence;

[Trait("Category", "Unit")]
public class UsersDbContextRepositoryTests : IDisposable
{
    private readonly UsersDbContext _context;

    public UsersDbContextRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new UsersDbContext(options, null!);
    }

    [Fact]
    public void GetRepository_ForUser_ShouldReturnSelf()
    {
        // Arrange & Act
        var repository = (IRepository<User, UserId>)_context;

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeSameAs(_context);
    }

    [Fact]
    public async Task TryFindAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.TryFindAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task TryFindAsync_WithNonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = new UserId(Guid.NewGuid());

        // Act
        var result = await _context.TryFindAsync(nonExistingId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_ShouldPersistUser()
    {
        // Arrange
        var user = new UserBuilder().Build();

        // Act
        _context.Add(user);
        await _context.SaveChangesAsync();

        // Assert
        var persisted = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        _context.Delete(user);
        await _context.SaveChangesAsync();

        // Assert
        var result = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task TryFindAsync_ShouldReturnUserWithEmail()
    {
        // Arrange
        var user = new UserBuilder().WithEmail("test@example.com").Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.TryFindAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task TryFindAsync_ShouldReturnUserWithUsername()
    {
        // Arrange
        var user = new UserBuilder().WithUsername("testuser").Build();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _context.TryFindAsync(user.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Value.Should().Be("testuser");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}


