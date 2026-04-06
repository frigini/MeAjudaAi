using FluentAssertions;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Modules.Users.Infrastructure.Services.LocalDevelopment;
using Xunit;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Infrastructure.Services.LocalDevelopment;

[Trait("Category", "Unit")]
public class LocalDevelopmentUserDomainServiceTests
{
    private readonly LocalDevelopmentUserDomainService _service = new();

    [Fact]
    public async Task CreateUserAsync_ShouldReturnSuccessWithGeneratedKeycloakId()
    {
        // Arrange
        var username = new Username("testuser");
        var email = new Email("test@example.com");
        var roles = new[] { "customer" };

        // Act
        var result = await _service.CreateUserAsync(
            username, email, "Test", "User", "password", roles, "11999999999");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be(username);
        result.Value.Email.Should().Be(email);
        result.Value.FirstName.Should().Be("Test");
        result.Value.LastName.Should().Be("User");
        result.Value.KeycloakId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SyncUserWithKeycloakAsync_ShouldAlwaysReturnSuccess()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var result = await _service.SyncUserWithKeycloakAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUserInKeycloakAsync_ShouldAlwaysReturnSuccess()
    {
        // Arrange
        var userId = new UserId(Guid.NewGuid());

        // Act
        var result = await _service.DeactivateUserInKeycloakAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
