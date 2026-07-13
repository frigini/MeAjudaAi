using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.Contracts.Unit.DTOs;

/// <summary>
/// Testes unitários para DTOs do módulo Locations em Shared.Contracts
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Contracts")]
public class ModuleLocationDtosTests
{
    #region ModuleAddressDto Tests

    [Fact]
    public void ModuleAddressDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var coordinates = new ModuleCoordinatesDto(Latitude: -23.5505199, Longitude: -46.6333094);
        var dto = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP", "Apto 101", coordinates);

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleAddressDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void ModuleAddressDto_WithoutOptionalFields_ShouldSerialize()
    {
        // Arrange
        var dto = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP", null, null);

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleAddressDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Complement.Should().BeNull();
        deserialized.Coordinates.Should().BeNull();
    }

    [Fact]
    public void ModuleAddressDto_ShouldBeImmutable()
    {
        // Arrange
        var original = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP");

        // Act
        var modified = original with { Street = "Rua Augusta" };

        // Assert
        original.Street.Should().Be("Avenida Paulista");
        modified.Street.Should().Be("Rua Augusta");
        original.Should().NotBe(modified);
    }

    [Fact]
    public void ModuleAddressDto_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var dto1 = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP");
        var dto2 = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP");

        // Act & Assert
        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
    }

    [Theory]
    [InlineData("01310-100", "São Paulo", "SP")]
    [InlineData("40301-110", "Salvador", "BA")]
    [InlineData("30140-071", "Belo Horizonte", "MG")]
    public void ModuleAddressDto_ShouldSupportMultipleCities(string cep, string city, string state)
    {
        // Arrange & Act
        var dto = new ModuleAddressDto(cep, "Test Street", "Test Neighborhood", city, state);

        // Assert
        dto.Cep.Should().Be(cep);
        dto.City.Should().Be(city);
        dto.State.Should().Be(state);
    }

    #endregion

    #region ModuleCoordinatesDto Tests

    [Fact]
    public void ModuleCoordinatesDto_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var dto = new ModuleCoordinatesDto(Latitude: -23.5505199, Longitude: -46.6333094);

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleCoordinatesDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Latitude.Should().BeApproximately(-23.5505199, 0.0000001);
        deserialized.Longitude.Should().BeApproximately(-46.6333094, 0.0000001);
    }

    [Fact]
    public void ModuleCoordinatesDto_ShouldBeImmutable()
    {
        // Arrange
        var original = new ModuleCoordinatesDto(Latitude: -23.5505199, Longitude: -46.6333094);

        // Act
        var modified = original with { Latitude = -22.9068 };

        // Assert
        original.Latitude.Should().BeApproximately(-23.5505199, 0.0000001);
        modified.Latitude.Should().BeApproximately(-22.9068, 0.0000001);
        original.Should().NotBe(modified);
    }

    [Fact]
    public void ModuleCoordinatesDto_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var dto1 = new ModuleCoordinatesDto(Latitude: -23.5505199, Longitude: -46.6333094);
        var dto2 = new ModuleCoordinatesDto(Latitude: -23.5505199, Longitude: -46.6333094);

        // Act & Assert
        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
    }

    [Theory]
    [InlineData(-90.0, -180.0)]
    [InlineData(90.0, 180.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(-23.5505199, -46.6333094)]
    [InlineData(40.7128, -74.0060)]
    public void ModuleCoordinatesDto_ShouldSupportValidCoordinateRanges(double lat, double lon)
    {
        // Arrange & Act
        var dto = new ModuleCoordinatesDto(Latitude: lat, Longitude: lon);

        // Assert
        dto.Latitude.Should().BeApproximately(lat, 0.0000001);
        dto.Longitude.Should().BeApproximately(lon, 0.0000001);
    }

    #endregion

    #region Serialization Behavior Tests

    [Fact]
    public void ModuleAddressDto_WithCoordinates_ShouldMaintainRelationship()
    {
        // Arrange
        var coordinates = new ModuleCoordinatesDto(Latitude: -23.5505199, Longitude: -46.6333094);
        var address = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP", null, coordinates);

        // Act
        var json = JsonSerializer.Serialize(address);
        var deserialized = JsonSerializer.Deserialize<ModuleAddressDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Coordinates.Should().NotBeNull();
        deserialized.Coordinates!.Latitude.Should().BeApproximately(coordinates.Latitude, 0.0000001);
    }

    [Fact]
    public void ModuleAddressDto_ShouldSerializeWithCamelCase()
    {
        // Arrange
        var dto = new ModuleAddressDto("01310-100", "Avenida Paulista", "Bela Vista", "Sao Paulo", "SP");
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Act
        var json = JsonSerializer.Serialize(dto, options);

        // Assert
        json.Should().Contain("\"cep\":\"01310-100\"");
        json.Should().Contain("\"street\":\"Avenida Paulista\"");
        json.Should().Contain("\"neighborhood\":\"Bela Vista\"");
        json.Should().Contain("\"city\":\"Sao Paulo\"");
        json.Should().Contain("\"state\":\"SP\"");
    }

    #endregion
}
