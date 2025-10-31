using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Users.Tests.Builders;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Persistence;

/// <summary>
/// Unit tests for IUserRepository interface contract validation.
/// Note: These tests use mocks to verify interface behavior contracts,
/// not the concrete UserRepository implementation. Actual repository 
/// implementation testing should be done in integration tests with real database.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Infrastructure")]
public class UserRepositoryTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;

    public UserRepositoryTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
    }

    [Fact]
    public async Task AddAsync_WithValidUser_ShouldCallRepositoryMethod()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFullName("John", "Doe")
            .WithKeycloakId("keycloak123")
            .Build();

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockUserRepository.Object.AddAsync(user);

        // Assert
        _mockUserRepository.Verify(x => x.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFullName("John", "Doe")
            .WithKeycloakId("keycloak123")
            .Build();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _mockUserRepository.Object.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Value.Should().Be("test@example.com");
        result.Username.Value.Should().Be("testuser");
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.KeycloakId.Should().Be("keycloak123");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = new UserId(Guid.NewGuid());

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepository.Object.GetByIdAsync(nonExistentId);

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
            .WithUsername("testuser")
            .WithFullName("John", "Doe")
            .WithKeycloakId("keycloak123")
            .Build();

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _mockUserRepository.Object.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Username.Value.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        var nonExistentEmail = new Email("nonexistent@example.com");

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(nonExistentEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepository.Object.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
    {
        // Arrange
        var username = new Username("testuser");
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername(username)
            .WithFullName("John", "Doe")
            .WithKeycloakId("keycloak123")
            .Build();

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _mockUserRepository.Object.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistentUsername_ShouldReturnNull()
    {
        // Arrange
        var nonExistentUsername = new Username("nonexistent");

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(nonExistentUsername, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepository.Object.GetByUsernameAsync(nonExistentUsername);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithExistingKeycloakId_ShouldReturnUser()
    {
        // Arrange
        var keycloakId = "keycloak123";
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFullName("John", "Doe")
            .WithKeycloakId(keycloakId)
            .Build();

        _mockUserRepository
            .Setup(x => x.GetByKeycloakIdAsync(keycloakId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _mockUserRepository.Object.GetByKeycloakIdAsync(keycloakId);

        // Assert
        result.Should().NotBeNull();
        result!.KeycloakId.Should().Be(keycloakId);
        result.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByKeycloakIdAsync_WithNonExistentKeycloakId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentKeycloakId = "nonexistent";

        _mockUserRepository
            .Setup(x => x.GetByKeycloakIdAsync(nonExistentKeycloakId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _mockUserRepository.Object.GetByKeycloakIdAsync(nonExistentKeycloakId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_WithValidParameters_ShouldReturnPagedResults()
    {
        // Arrange
        var users = new List<User>();
        for (int i = 1; i <= 5; i++)
        {
            var user = new UserBuilder()
                .WithEmail($"user{i}@example.com")
                .WithUsername($"user{i}")
                .WithFullName($"User{i}", "Test")
                .WithKeycloakId($"keycloak{i}")
                .Build();
            users.Add(user);
        }

        var expectedResult = (users.Take(3).ToList() as IReadOnlyList<User>, 5);

        _mockUserRepository
            .Setup(x => x.GetPagedAsync(1, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserRepository.Object.GetPagedAsync(pageNumber: 1, pageSize: 3);

        // Assert
        result.Users.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetPagedAsync_WithEmptyDatabase_ShouldReturnEmptyResults()
    {
        // Arrange
        var expectedResult = (new List<User>() as IReadOnlyList<User>, 0);

        _mockUserRepository
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserRepository.Object.GetPagedAsync(pageNumber: 1, pageSize: 10);

        // Assert
        result.Users.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetPagedWithSearchAsync_WithSearchTerm_ShouldReturnFilteredResults()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder().WithEmail("john@example.com").WithUsername("john").WithFullName("John", "Doe").Build(),
            new UserBuilder().WithEmail("jane@example.com").WithUsername("jane").WithFullName("Jane", "Smith").Build()
        };

        var expectedResult = (users as IReadOnlyList<User>, 2);

        _mockUserRepository
            .Setup(x => x.GetPagedWithSearchAsync(1, 10, "john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _mockUserRepository.Object.GetPagedWithSearchAsync(pageNumber: 1, pageSize: 10, searchTerm: "john");

        // Assert
        result.Users.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_ShouldCallRepositoryMethod()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFullName("John", "Doe")
            .WithKeycloakId("keycloak123")
            .Build();

        _mockUserRepository
            .Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockUserRepository.Object.UpdateAsync(user);

        // Assert
        _mockUserRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingUser_ShouldCallRepositoryMethod()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        _mockUserRepository
            .Setup(x => x.DeleteAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockUserRepository.Object.DeleteAsync(userId);

        // Assert
        _mockUserRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        _mockUserRepository
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockUserRepository.Object.ExistsAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        _mockUserRepository
            .Setup(x => x.ExistsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mockUserRepository.Object.ExistsAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void UserRepository_ShouldImplementIUserRepository()
    {
        // Arrange & Act
        var userRepositoryType = typeof(UserRepository);

        // Assert
        userRepositoryType.Should().Implement<IUserRepository>();
    }
}
