using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Tests.Mocks;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Users.Tests.Integration.Infrastructure;

public class UserRepositoryTests : DatabaseTestBase
{
    private UserRepository _repository = null!;
    private UsersDbContext _context = null!;

    private async Task InitializeInternalAsync()
    {
        await base.InitializeAsync();

        var options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _context = new UsersDbContext(options);
        await _context.Database.MigrateAsync();

        var mockDateTimeProvider = new MockDateTimeProvider();
        _repository = new UserRepository(_context, mockDateTimeProvider);
    }

    [Fact]
    public async Task AddAsync_WithValidUser_ShouldPersistUser()
    {
        // Arrange
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .WithFirstName("John")
            .WithLastName("Doe")
            .WithKeycloakId("keycloak-123")
            .Build();

        // Act
        await AddUserAndSaveAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Username.Value.Should().Be("testuser");
        savedUser.Email.Value.Should().Be("test@example.com");
        savedUser.FirstName.Should().Be("John");
        savedUser.LastName.Should().Be("Doe");
        savedUser.KeycloakId.Should().Be("keycloak-123");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await AddUserAndSaveAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = UserId.New();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = new Email("test@example.com");
        var user = new UserBuilder()
            .WithEmail(email)
            .Build();
        await AddUserAndSaveAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var nonExistingEmail = new Email("nonexisting@example.com");

        // Act
        var result = await _repository.GetByEmailAsync(nonExistingEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
    {
        // Arrange
        var username = new Username("testuser");
        var user = new UserBuilder()
            .WithUsername(username)
            .Build();
        await AddUserAndSaveAsync(user);

        // Act
        var result = await _repository.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ShouldReturnNull()
    {
        // Arrange
        var nonExistingUsername = new Username("nonexisting");

        // Act
        var result = await _repository.GetByUsernameAsync(nonExistingUsername);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithExistingKeycloakId_ShouldReturnUser()
    {
        // Arrange
        var keycloakId = "keycloak-123";
        var user = new UserBuilder()
            .WithKeycloakId(keycloakId)
            .Build();
        await AddUserAndSaveAsync(user);

        // Act
        var result = await _repository.GetByKeycloakIdAsync(keycloakId);

        // Assert
        result.Should().NotBeNull();
        result!.KeycloakId.Should().Be(keycloakId);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await AddUserAndSaveAsync(user);

        // Act
        user.UpdateProfile("Updated", "Name");
        await UpdateUserAndSaveAsync(user);

        // Assert
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("Name");
        updatedUser.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingUser_ShouldSoftDeleteUser()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await AddUserAndSaveAsync(user);

        // Act
        await _repository.DeleteAsync(user.Id);
        await _context.SaveChangesAsync();

        // Assert
        // Não deve ser encontrado por consultas normais (exclusão lógica)
        var foundUser = await _repository.GetByIdAsync(user.Id);
        foundUser.Should().BeNull();

        // Mas deve existir no banco de dados com IsDeleted = true
        var deletedUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        deletedUser.Should().NotBeNull();
        deletedUser!.IsDeleted.Should().BeTrue();
        deletedUser.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var user = new UserBuilder().Build();
        await AddUserAndSaveAsync(user);

        // Act
        var exists = await _repository.ExistsAsync(user.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingUser_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = UserId.New();

        // Act
        var exists = await _repository.ExistsAsync(nonExistingId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_WithUsers_ShouldReturnPagedResults()
    {
        // Arrange
        var users = new UserBuilder().BuildMany(5).ToList();
        foreach (var user in users)
        {
            await AddUserAndSaveAsync(user);
        }

        // Act
        var (pagedUsers, totalCount) = await _repository.GetPagedAsync(1, 3);

        // Assert
        pagedUsers.Should().HaveCount(3);
        totalCount.Should().Be(5);
        pagedUsers.Should().AllSatisfy(u => u.Should().NotBeNull());
    }

    [Fact]
    public async Task GetPagedAsync_WithEmptyDatabase_ShouldReturnEmptyResults()
    {
        // Act
        var (pagedUsers, totalCount) = await _repository.GetPagedAsync(1, 10);

        // Assert
        pagedUsers.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    public override async ValueTask InitializeAsync()
    {
        await InitializeInternalAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await DisposeInternalAsync();
    }

    private async Task DisposeInternalAsync()
    {
        await _context.DisposeAsync();
        await base.DisposeAsync();
    }

    // Método auxiliar para adicionar usuário e persistir
    private async Task AddUserAndSaveAsync(User user)
    {
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    // Método auxiliar para atualizar usuário e persistir
    private async Task UpdateUserAndSaveAsync(User user)
    {
        await _repository.UpdateAsync(user);
        await _context.SaveChangesAsync();
    }
}
