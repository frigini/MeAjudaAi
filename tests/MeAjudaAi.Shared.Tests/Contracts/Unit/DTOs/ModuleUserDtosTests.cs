using MeAjudaAi.Contracts.Modules.Users.DTOs;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit.DTOs;

/// <summary>
/// Testes unitários para DTOs do módulo Users em Shared.Contracts
/// Valida serialização, imutabilidade e estrutura de dados
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ModuleUserDtosTests
{
    #region ModuleUserDto Tests

    [Fact]
    public void ModuleUserDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new ModuleUserDto(
            Id: Guid.NewGuid(),
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleUserDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void ModuleUserDto_ShouldBeImmutable()
    {
        // Arrange
        var dto = new ModuleUserDto(
            Id: Guid.NewGuid(),
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        // Act
        var modified = dto with { Username = "modified" };

        // Assert
        dto.Username.Should().Be("testuser");
        modified.Username.Should().Be("modified");
        dto.Should().NotBe(modified);
    }

    [Fact]
    public void ModuleUserDto_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new ModuleUserDto(id, "testuser", "test@example.com", "Test", "User", "Test User");
        var dto2 = new ModuleUserDto(id, "testuser", "test@example.com", "Test", "User", "Test User");

        // Act & Assert
        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
    }

    [Fact]
    public void ModuleUserDto_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var dto1 = new ModuleUserDto(Guid.NewGuid(), "user1", "user1@example.com", "User", "One", "User One");
        var dto2 = new ModuleUserDto(Guid.NewGuid(), "user2", "user2@example.com", "User", "Two", "User Two");

        // Act & Assert
        dto1.Should().NotBe(dto2);
        (dto1 == dto2).Should().BeFalse();
    }

    [Fact]
    public void ModuleUserDto_ShouldHaveConsistentHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new ModuleUserDto(id, "testuser", "test@example.com", "Test", "User", "Test User");
        var dto2 = new ModuleUserDto(id, "testuser", "test@example.com", "Test", "User", "Test User");

        // Act & Assert
        dto1.GetHashCode().Should().Be(dto2.GetHashCode());
    }

    [Fact]
    public void ModuleUserDto_ShouldSerializeWithCamelCase()
    {
        // Arrange
        var dto = new ModuleUserDto(
            Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            "testuser", "test@example.com", "Test", "User", "Test User");

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var json = JsonSerializer.Serialize(dto, options);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"username\":\"testuser\"");
        json.Should().Contain("\"email\":\"test@example.com\"");
        json.Should().Contain("\"firstName\":\"Test\"");
        json.Should().Contain("\"lastName\":\"User\"");
        json.Should().Contain("\"fullName\":\"Test User\"");
    }

    #endregion

    #region ModuleUserBasicDto Tests

    [Fact]
    public void ModuleUserBasicDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new ModuleUserBasicDto(Guid.NewGuid(), "testuser", "test@example.com", true);

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleUserBasicDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void ModuleUserBasicDto_ShouldBeImmutable()
    {
        // Arrange
        var dto = new ModuleUserBasicDto(Guid.NewGuid(), "testuser", "test@example.com", true);

        // Act
        var modified = dto with { IsActive = false };

        // Assert
        dto.IsActive.Should().BeTrue();
        modified.IsActive.Should().BeFalse();
        dto.Should().NotBe(modified);
    }

    [Fact]
    public void ModuleUserBasicDto_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new ModuleUserBasicDto(id, "testuser", "test@example.com", true);
        var dto2 = new ModuleUserBasicDto(id, "testuser", "test@example.com", true);

        // Act & Assert
        dto1.Should().Be(dto2);
    }

    [Fact]
    public void ModuleUserBasicDto_ShouldContainOnlyEssentialFields()
    {
        // Arrange
        var dto = new ModuleUserBasicDto(Guid.NewGuid(), "testuser", "test@example.com", true);

        // Act
        var json = JsonSerializer.Serialize(dto);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert
        jsonElement.EnumerateObject().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ModuleUserBasicDto_ShouldSupportBothActiveStates(bool isActive)
    {
        // Arrange & Act
        var dto = new ModuleUserBasicDto(Guid.NewGuid(), "testuser", "test@example.com", isActive);

        // Assert
        dto.IsActive.Should().Be(isActive);
    }

    #endregion

    #region Cross-DTO Tests

    [Fact]
    public void ModuleUserDto_AndBasicDto_ShouldShareCommonProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fullDto = new ModuleUserDto(id, "testuser", "test@example.com", "Test", "User", "Test User");
        var basicDto = new ModuleUserBasicDto(id, "testuser", "test@example.com", true);

        // Assert
        fullDto.Id.Should().Be(basicDto.Id);
        fullDto.Username.Should().Be(basicDto.Username);
        fullDto.Email.Should().Be(basicDto.Email);
    }

    #endregion
}
