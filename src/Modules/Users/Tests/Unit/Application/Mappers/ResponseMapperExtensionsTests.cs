using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class ResponseMapperExtensionsTests
{
    private static UserDto CreateTestUser(
        string? deviceToken = "token123",
        string? phoneNumber = "+5511999999999",
        bool isActive = true,
        string email = "test@example.com",
        string username = "testuser")
    {
        return new UserDto(
            Id: Guid.NewGuid(),
            Username: username,
            Email: email,
            FirstName: "John",
            LastName: "Doe",
            FullName: "John Doe",
            DeviceToken: deviceToken,
            PhoneNumber: phoneNumber,
            IsActive: isActive,
            KeycloakId: Guid.NewGuid().ToString(),
            CreatedAt: DateTime.UtcNow.AddDays(-30),
            UpdatedAt: DateTime.UtcNow);
    }

    [Fact]
    public void ToContract_WithCompleteUser_ShouldMapAllProperties()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var contract = user.ToContract();

        // Assert
        contract.Should().NotBeNull();
        contract.Id.Should().Be(user.Id);
        contract.Username.Should().Be("testuser");
        contract.Email.Should().Be("test@example.com");
        contract.FirstName.Should().Be("John");
        contract.LastName.Should().Be("Doe");
        contract.FullName.Should().Be("John Doe");
        contract.DeviceToken.Should().Be("token123");
    }

    [Fact]
    public void ToContract_WithNullDeviceToken_ShouldMapNull()
    {
        // Arrange
        var user = CreateTestUser(deviceToken: null);

        // Act
        var contract = user.ToContract();

        // Assert
        contract.DeviceToken.Should().BeNull();
    }

    [Fact]
    public void ToBasicContract_WithCompleteUser_ShouldMapAllProperties()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var contract = user.ToBasicContract();

        // Assert
        contract.Should().NotBeNull();
        contract.Id.Should().Be(user.Id);
        contract.Username.Should().Be("testuser");
        contract.Email.Should().Be("test@example.com");
        contract.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToBasicContract_Collection_ShouldMapAllItems()
    {
        // Arrange
        var users = new List<UserDto>
        {
            CreateTestUser(username: "user1", email: "user1@test.com"),
            CreateTestUser(username: "user2", email: "user2@test.com"),
            CreateTestUser(username: "user3", email: "user3@test.com")
        };

        // Act
        var contracts = users.ToBasicContract();

        // Assert
        contracts.Should().HaveCount(3);
        contracts[0].Username.Should().Be("user1");
        contracts[0].Email.Should().Be("user1@test.com");
        contracts[1].Username.Should().Be("user2");
        contracts[1].Email.Should().Be("user2@test.com");
        contracts[2].Username.Should().Be("user3");
        contracts[2].Email.Should().Be("user3@test.com");
    }

    [Fact]
    public void ToBasicContract_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var users = Enumerable.Empty<UserDto>();

        // Act
        var contracts = users.ToBasicContract();

        // Assert
        contracts.Should().BeEmpty();
    }
}
