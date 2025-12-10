using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;

namespace MeAjudaAi.Shared.Tests.Unit.Contracts.DTOs;

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

        // Act - Try to create new instance with modified value
        var modified = dto with { Username = "modified" };

        // Assert - Original should be unchanged
        dto.Username.Should().Be("testuser");
        modified.Username.Should().Be("modified");
        dto.Should().NotBe(modified);
    }

    [Fact]
    public void ModuleUserDto_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new ModuleUserDto(
            Id: id,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        var dto2 = new ModuleUserDto(
            Id: id,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        // Act & Assert
        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
    }

    [Fact]
    public void ModuleUserDto_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var dto1 = new ModuleUserDto(
            Id: Guid.NewGuid(),
            Username: "user1",
            Email: "user1@example.com",
            FirstName: "User",
            LastName: "One",
            FullName: "User One"
        );

        var dto2 = new ModuleUserDto(
            Id: Guid.NewGuid(),
            Username: "user2",
            Email: "user2@example.com",
            FirstName: "User",
            LastName: "Two",
            FullName: "User Two"
        );

        // Act & Assert
        dto1.Should().NotBe(dto2);
        (dto1 == dto2).Should().BeFalse();
    }

    [Fact]
    public void ModuleUserDto_ShouldHaveConsistentHashCode()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto1 = new ModuleUserDto(
            Id: id,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        var dto2 = new ModuleUserDto(
            Id: id,
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        // Act & Assert
        dto1.GetHashCode().Should().Be(dto2.GetHashCode());
    }

    [Fact]
    public void ModuleUserDto_ShouldSerializeWithCamelCase()
    {
        // Arrange
        var dto = new ModuleUserDto(
            Id: Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            Username: "testuser",
            Email: "test@example.com",
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
        var dto = new ModuleUserBasicDto(
            Id: Guid.NewGuid(),
            Username: "testuser",
            Email: "test@example.com",
            IsActive: true
        );

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
        var dto = new ModuleUserBasicDto(
            Id: Guid.NewGuid(),
            Username: "testuser",
            Email: "test@example.com",
            IsActive: true
        );

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
        var dto1 = new ModuleUserBasicDto(
            Id: id,
            Username: "testuser",
            Email: "test@example.com",
            IsActive: true
        );

        var dto2 = new ModuleUserBasicDto(
            Id: id,
            Username: "testuser",
            Email: "test@example.com",
            IsActive: true
        );

        // Act & Assert
        dto1.Should().Be(dto2);
    }

    [Fact]
    public void ModuleUserBasicDto_ShouldContainOnlyEssentialFields()
    {
        // Arrange
        var dto = new ModuleUserBasicDto(
            Id: Guid.NewGuid(),
            Username: "testuser",
            Email: "test@example.com",
            IsActive: true
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

        // Assert - Should have exactly 4 properties
        jsonElement.EnumerateObject().Should().HaveCount(4);
        jsonElement.TryGetProperty("Id", out _).Should().BeTrue();
        jsonElement.TryGetProperty("Username", out _).Should().BeTrue();
        jsonElement.TryGetProperty("Email", out _).Should().BeTrue();
        jsonElement.TryGetProperty("IsActive", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ModuleUserBasicDto_ShouldSupportBothActiveStates(bool isActive)
    {
        // Arrange & Act
        var dto = new ModuleUserBasicDto(
            Id: Guid.NewGuid(),
            Username: "testuser",
            Email: "test@example.com",
            IsActive: isActive
        );

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
        var username = "testuser";
        var email = "test@example.com";

        var fullDto = new ModuleUserDto(
            Id: id,
            Username: username,
            Email: email,
            FirstName: "Test",
            LastName: "User",
            FullName: "Test User"
        );

        var basicDto = new ModuleUserBasicDto(
            Id: id,
            Username: username,
            Email: email,
            IsActive: true
        );

        // Assert - Common properties should match
        fullDto.Id.Should().Be(basicDto.Id);
        fullDto.Username.Should().Be(basicDto.Username);
        fullDto.Email.Should().Be(basicDto.Email);
    }

    #endregion
}
