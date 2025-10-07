using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Tests.Integration.Infrastructure;

[Collection("Database")]
public class UserRepositoryIntegrationTests : BaseIntegrationTest
{
    private readonly UsersDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests(TestApplicationFactory factory) : base(factory)
    {
        _context = Services.GetRequiredService<UsersDbContext>();
        var logger = Services.GetRequiredService<ILogger<UserRepository>>();
        _repository = new UserRepository(_context, logger);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistUserToDatabase()
    {
        // Arrange
        var user = User.Create(
            Email.Create("test@example.com"),
            Username.Create("testuser"),
            UserProfile.Create("Test", "User", "+5511999999999")
        );

        // Act
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Assert
        var savedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == "test@example.com");
        
        savedUser.Should().NotBeNull();
        savedUser!.Email.Value.Should().Be("test@example.com");
        savedUser.Username.Value.Should().Be("testuser");
        savedUser.Profile.FirstName.Should().Be("Test");
        savedUser.Profile.LastName.Should().Be("User");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = User.Create(
            Email.Create("existing@example.com"),
            Username.Create("existinguser"),
            UserProfile.Create("Existing", "User", "+5511999999999")
        );
        
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Value.Should().Be("existing@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = UserId.Create(Guid.NewGuid());

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = Email.Create("byemail@example.com");
        var user = User.Create(
            email,
            Username.Create("byemailuser"),
            UserProfile.Create("By", "Email", "+5511999999999")
        );
        
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Username.Value.Should().Be("byemailuser");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var nonExistentEmail = Email.Create("nonexistent@example.com");

        // Act
        var result = await _repository.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
    {
        // Arrange
        var username = Username.Create("byusername");
        var user = User.Create(
            Email.Create("byusername@example.com"),
            username,
            UserProfile.Create("By", "Username", "+5511999999999")
        );
        
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsernameAsync(username);

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be(username);
        result.Email.Value.Should().Be("byusername@example.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ShouldReturnNull()
    {
        // Arrange
        var nonExistentUsername = Username.Create("nonexistent");

        // Act
        var result = await _repository.GetByUsernameAsync(nonExistentUsername);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        var user = User.Create(
            Email.Create("update@example.com"),
            Username.Create("updateuser"),
            UserProfile.Create("Original", "User", "+5511999999999")
        );
        
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Modify the user
        var newProfile = UserProfile.Create("Updated", "User", "+5511888888888");
        user.UpdateProfile(newProfile);

        // Act
        _repository.Update(user);
        await _context.SaveChangesAsync();

        // Assert
        var updatedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        
        updatedUser.Should().NotBeNull();
        updatedUser!.Profile.FirstName.Should().Be("Updated");
        updatedUser.Profile.PhoneNumber.Value.Should().Be("+5511888888888");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var user = User.Create(
            Email.Create("delete@example.com"),
            Username.Create("deleteuser"),
            UserProfile.Create("Delete", "User", "+5511999999999")
        );
        
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(user);
        await _context.SaveChangesAsync();

        // Assert
        var deletedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);
        
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create(
            Email.Create("exists@example.com"),
            Username.Create("existsuser"),
            UserProfile.Create("Exists", "User", "+5511999999999")
        );
        
        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var exists = await _repository.ExistsAsync(user.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingUser_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = UserId.Create(Guid.NewGuid());

        // Act
        var exists = await _repository.ExistsAsync(nonExistentId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var user1 = User.Create(
            Email.Create("user1@example.com"),
            Username.Create("user1"),
            UserProfile.Create("User", "One", "+5511999999999")
        );
        
        var user2 = User.Create(
            Email.Create("user2@example.com"),
            Username.Create("user2"),
            UserProfile.Create("User", "Two", "+5511888888888")
        );

        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _context.SaveChangesAsync();

        // Act
        var users = await _repository.GetAllAsync();

        // Assert
        users.Should().HaveCountGreaterOrEqualTo(2);
        users.Should().Contain(u => u.Email.Value == "user1@example.com");
        users.Should().Contain(u => u.Email.Value == "user2@example.com");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var users = new List<User>();
        for (int i = 1; i <= 5; i++)
        {
            var user = User.Create(
                Email.Create($"user{i}@example.com"),
                Username.Create($"user{i}"),
                UserProfile.Create($"User", $"{i}", $"+551199999999{i}")
            );
            users.Add(user);
            await _repository.AddAsync(user);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(1, 3);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().BeGreaterOrEqualTo(5);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Theory]
    [InlineData("test@domain.com", "testuser", true)]
    [InlineData("", "testuser", false)]
    [InlineData("test@domain.com", "", false)]
    [InlineData("invalid-email", "testuser", false)]
    public async Task EmailValidation_ShouldWorkCorrectly(string emailValue, string usernameValue, bool shouldSucceed)
    {
        // Arrange & Act
        if (shouldSucceed)
        {
            var email = Email.Create(emailValue);
            var username = Username.Create(usernameValue);
            var user = User.Create(
                email,
                username,
                UserProfile.Create("Test", "User", "+5511999999999")
            );

            await _repository.AddAsync(user);
            var result = await _context.SaveChangesAsync();

            // Assert
            result.Should().BeGreaterThan(0);
        }
        else
        {
            // Assert
            var act = () =>
            {
                if (string.IsNullOrEmpty(emailValue) || emailValue == "invalid-email")
                {
                    Email.Create(emailValue);
                }
                if (string.IsNullOrEmpty(usernameValue))
                {
                    Username.Create(usernameValue);
                }
            };

            act.Should().Throw<Exception>();
        }
    }

    protected override async Task CleanupAsync()
    {
        // Cleanup all test data
        var testUsers = await _context.Users
            .Where(u => u.Email.Value.Contains("@example.com"))
            .ToListAsync();

        _context.Users.RemoveRange(testUsers);
        await _context.SaveChangesAsync();
    }
}
