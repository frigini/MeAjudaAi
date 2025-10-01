using MeAjudaAi.Modules.Users.Tests.Infrastructure;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Time;

namespace MeAjudaAi.Modules.Users.Tests.Integration.Services;

[Collection("UsersIntegrationTests")]
public class UsersModuleApiIntegrationTests : UsersIntegrationTestBase
{
    private IUsersModuleApi _moduleApi = null!;

    protected override Task OnModuleInitializeAsync(IServiceProvider serviceProvider)
    {
        _moduleApi = GetService<IUsersModuleApi>();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = await CreateUserAsync(
            username: "integrationtest",
            email: "integration@test.com",
            firstName: "Integration",
            lastName: "Test"
        );

        // Act
        var result = await _moduleApi.GetUserByIdAsync(user.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id.Value);
        result.Value.Username.Should().Be("integrationtest");
        result.Value.Email.Should().Be("integration@test.com");
        result.Value.FirstName.Should().Be("Integration");
        result.Value.LastName.Should().Be("Test");
        result.Value.FullName.Should().Be("Integration Test");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.GetUserByIdAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = await CreateUserAsync(
            username: "emailtest",
            email: "emailtest@example.com",
            firstName: "Email",
            lastName: "Test"
        );

        // Act
        var result = await _moduleApi.GetUserByEmailAsync("emailtest@example.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id.Value);
        result.Value.Username.Should().Be("emailtest");
        result.Value.Email.Should().Be("emailtest@example.com");
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Arrange
        var nonExistentEmail = $"nonexistent_{UuidGenerator.NewIdStringCompact()}@test.com";

        // Act
        var result = await _moduleApi.GetUserByEmailAsync(nonExistentEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithMultipleExistingUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = await CreateUserAsync("batchuser1", "batch1@test.com", "Batch", "User1");
        var user2 = await CreateUserAsync("batchuser2", "batch2@test.com", "Batch", "User2");
        var user3 = await CreateUserAsync("batchuser3", "batch3@test.com", "Batch", "User3");

        var userIds = new List<Guid> { user1.Id.Value, user2.Id.Value, user3.Id.Value };

        // Act
        var result = await _moduleApi.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        result.Value.Should().Contain(u => u.Id == user1.Id.Value && u.Username == "batchuser1");
        result.Value.Should().Contain(u => u.Id == user2.Id.Value && u.Username == "batchuser2");
        result.Value.Should().Contain(u => u.Id == user3.Id.Value && u.Username == "batchuser3");

        // Verify all users are marked as active
        result.Value.Should().AllSatisfy(user => user.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithMixOfExistingAndNonExistentUsers_ShouldReturnOnlyExisting()
    {
        // Arrange
        var existingUser = await CreateUserAsync("mixedtest", "mixed@test.com", "Mixed", "Test");
        var nonExistentId = UuidGenerator.NewId();

        var userIds = new List<Guid> { existingUser.Id.Value, nonExistentId };

        // Act
        var result = await _moduleApi.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Single().Id.Should().Be(existingUser.Id.Value);
    }

    [Fact]
    public async Task UserExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var user = await CreateUserAsync("existstest", "exists@test.com", "Exists", "Test");

        // Act
        var result = await _moduleApi.UserExistsAsync(user.Id.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task UserExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = UuidGenerator.NewId();

        // Act
        var result = await _moduleApi.UserExistsAsync(nonExistentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        await CreateUserAsync("emailexists", "emailexists@test.com", "Email", "Exists");

        // Act
        var result = await _moduleApi.EmailExistsAsync("emailexists@test.com");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentEmail = $"nonexistent_{UuidGenerator.NewIdStringCompact()}@test.com";

        // Act
        var result = await _moduleApi.EmailExistsAsync(nonExistentEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }



    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        await CreateUserAsync("usernametest", "usernametest@test.com", "Username", "Test");

        // Act
        var result = await _moduleApi.UsernameExistsAsync("usernametest");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue(); // Usuário existe, então deve retornar true
    }

    [Fact]
    public async Task GetUsersBatchAsync_WithEmptyList_ShouldReturnEmptyResult()
    {
        // Arrange
        var emptyIds = new List<Guid>();

        // Act
        var result = await _moduleApi.GetUsersBatchAsync(emptyIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ModuleApi_ShouldWorkWithLargeUserBatch()
    {
        // Arrange
        var users = new List<Domain.Entities.User>();
        var userIds = new List<Guid>();

        // Cria 10 usuários para o teste em lote
        for (int i = 0; i < 10; i++)
        {
            var user = await CreateUserAsync(
                $"batchlarge{i}",
                $"batchlarge{i}@test.com",
                "Batch",
                $"Large{i}"
            );
            users.Add(user);
            userIds.Add(user.Id.Value);
        }

        // Act
        var result = await _moduleApi.GetUsersBatchAsync(userIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);

        foreach (var user in users)
        {
            result.Value.Should().Contain(u => u.Id == user.Id.Value);
        }
    }
}