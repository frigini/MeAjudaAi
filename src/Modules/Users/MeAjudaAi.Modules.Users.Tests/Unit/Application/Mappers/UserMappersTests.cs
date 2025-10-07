using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Tests.Builders;

namespace MeAjudaAi.Modules.Users.Tests.Unit.Application.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Users")]
[Trait("Layer", "Application")]
public class UserMappersTests
{
    [Fact]
    public void ToDto_WithValidUser_ShouldMapAllProperties()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("john.doe@example.com")
            .WithUsername("johndoe")
            .WithFullName("John", "Doe")
            .WithKeycloakId("keycloak-123")
            .Build();

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(user.Id.Value);
        dto.Username.Should().Be(user.Username.Value);
        dto.Email.Should().Be(user.Email.Value);
        dto.FirstName.Should().Be(user.FirstName);
        dto.LastName.Should().Be(user.LastName);
        dto.FullName.Should().Be(user.GetFullName());
        dto.KeycloakId.Should().Be(user.KeycloakId);
        dto.CreatedAt.Should().Be(user.CreatedAt);
        dto.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    [Fact]
    public void ToDto_WithUserWithEmptyFirstName_ShouldMapCorrectly()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFullName("", "Doe")
            .WithKeycloakId("keycloak-456")
            .Build();

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.FirstName.Should().Be("");
        dto.LastName.Should().Be("Doe");
        dto.FullName.Should().Be(user.GetFullName());
    }

    [Fact]
    public void ToDto_WithUserWithEmptyLastName_ShouldMapCorrectly()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .WithFullName("John", "")
            .WithKeycloakId("keycloak-789")
            .Build();

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("");
        dto.FullName.Should().Be(user.GetFullName());
    }

    [Fact]
    public void ToDto_WithSpecialCharactersInNames_ShouldMapCorrectly()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("jose@example.com") // Email válido sem caracteres especiais
            .WithUsername("jose_silva")
            .WithFullName("José Carlos", "da Silva")
            .WithKeycloakId("keycloak-special")
            .Build();

        // Act
        var dto = user.ToDto();

        // Assert
        dto.Should().NotBeNull();
        dto.FirstName.Should().Be("José Carlos");
        dto.LastName.Should().Be("da Silva");
        dto.FullName.Should().Be("José Carlos da Silva");
        dto.Email.Should().Be("jose@example.com"); // Email sem caracteres especiais
        dto.Username.Should().Be("jose_silva");
    }

    [Fact]
    public void ToDto_ShouldPreserveExactTimestamps()
    {
        // Arrange
        var createdAt = new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2023, 2, 20, 14, 45, 30, DateTimeKind.Utc);

        var user = new UserBuilder()
            .WithEmail("timestamp@example.com")
            .WithUsername("timestampuser")
            .WithFullName("Time", "Stamp")
            .WithKeycloakId("keycloak-time")
            .WithCreatedAt(createdAt)
            .WithUpdatedAt(updatedAt)
            .Build();

        // Act
        var dto = user.ToDto();

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}