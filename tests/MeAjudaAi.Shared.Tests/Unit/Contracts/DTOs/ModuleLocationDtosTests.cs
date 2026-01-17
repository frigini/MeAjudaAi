using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;

namespace MeAjudaAi.Shared.Tests.Unit.Contracts.DTOs;

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
        var coordinates = new ModuleCoordinatesDto(
            Latitude: -23.5505199,
            Longitude: -46.6333094
        );

        var dto = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP",
            Complement: "Apto 101",
            Coordinates: coordinates
        );

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
        var dto = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP",
            Complement: null,
            Coordinates: null
        );

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
        var original = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP"
        );

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
        var dto1 = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP"
        );

        var dto2 = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP"
        );

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
        var dto = new ModuleAddressDto(
            Cep: cep,
            Street: "Test Street",
            Neighborhood: "Test Neighborhood",
            City: city,
            State: state
        );

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
        var dto = new ModuleCoordinatesDto(
            Latitude: -23.5505199,
            Longitude: -46.6333094
        );

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
        var original = new ModuleCoordinatesDto(
            Latitude: -23.5505199,
            Longitude: -46.6333094
        );

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
        var dto1 = new ModuleCoordinatesDto(
            Latitude: -23.5505199,
            Longitude: -46.6333094
        );

        var dto2 = new ModuleCoordinatesDto(
            Latitude: -23.5505199,
            Longitude: -46.6333094
        );

        // Act & Assert
        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
    }

    [Theory]
    [InlineData(-90.0, -180.0)] // Min values
    [InlineData(90.0, 180.0)]   // Max values
    [InlineData(0.0, 0.0)]      // Null Island
    [InlineData(-23.5505199, -46.6333094)] // São Paulo
    [InlineData(40.7128, -74.0060)]        // New York
    public void ModuleCoordinatesDto_ShouldSupportValidCoordinateRanges(double lat, double lon)
    {
        // Arrange & Act
        var dto = new ModuleCoordinatesDto(
            Latitude: lat,
            Longitude: lon
        );

        // Assert
        dto.Latitude.Should().BeApproximately(lat, 0.0000001);
        dto.Longitude.Should().BeApproximately(lon, 0.0000001);
    }

    [Fact]
    public void ModuleCoordinatesDto_ShouldHandleHighPrecisionCoordinates()
    {
        // Arrange
        var dto = new ModuleCoordinatesDto(
            Latitude: -23.550519900000001,
            Longitude: -46.633309400000002
        );

        // Act
        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<ModuleCoordinatesDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Latitude.Should().BeApproximately(-23.550519900000001, 0.0000000001);
        deserialized.Longitude.Should().BeApproximately(-46.633309400000002, 0.0000000001);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ModuleAddressDto_WithCoordinates_ShouldMaintainRelationship()
    {
        // Arrange
        var coordinates = new ModuleCoordinatesDto(
            Latitude: -23.5505199,
            Longitude: -46.6333094
        );

        var address = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "São Paulo",
            State: "SP",
            Coordinates: coordinates
        );

        // Act
        var json = JsonSerializer.Serialize(address);
        var deserialized = JsonSerializer.Deserialize<ModuleAddressDto>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Coordinates.Should().NotBeNull();
        deserialized.Coordinates!.Latitude.Should().BeApproximately(coordinates.Latitude, 0.0000001);
        deserialized.Coordinates.Longitude.Should().BeApproximately(coordinates.Longitude, 0.0000001);
    }

    [Fact]
    public void ModuleAddressDto_ShouldSerializeWithCamelCase()
    {
        // Arrange
        var dto = new ModuleAddressDto(
            Cep: "01310-100",
            Street: "Avenida Paulista",
            Neighborhood: "Bela Vista",
            City: "Sao Paulo",
            State: "SP"
        );

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
