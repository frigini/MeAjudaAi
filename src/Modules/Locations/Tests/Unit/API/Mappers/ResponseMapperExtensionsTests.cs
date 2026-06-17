using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.DTOs;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Layer", "API")]
public class ResponseMapperExtensionsTests
{
    private static AllowedCityDto CreateTestDto(double serviceRadiusKm = 50.0)
        => new(
            Id: Guid.NewGuid(),
            CityName: "Muriaé",
            StateSigla: "MG",
            IbgeCode: 3143906,
            Latitude: -21.13,
            Longitude: -42.37,
            ServiceRadiusKm: serviceRadiusKm,
            IsActive: true,
            CreatedAt: DateTime.UtcNow.AddDays(-10),
            UpdatedAt: DateTime.UtcNow,
            CreatedBy: "admin@test.com",
            UpdatedBy: null);

    [Fact]
    public void ToContract_SingleDto_ShouldMapAllProperties()
    {
        // Arrange
        var dto = CreateTestDto(50.0);

        // Act
        var contract = dto.ToContract();

        // Assert
        contract.Id.Should().Be(dto.Id);
        contract.City.Should().Be("Muriaé");
        contract.State.Should().Be("MG");
        contract.Country.Should().Be("Brasil");
        contract.Latitude.Should().Be(-21.13);
        contract.Longitude.Should().Be(-42.37);
        contract.ServiceRadiusKm.Should().Be(50);
        contract.IsActive.Should().BeTrue();
        contract.CreatedAt.Should().Be(dto.CreatedAt);
        contract.UpdatedAt.Should().Be(dto.UpdatedAt);
    }

    [Fact]
    public void ToContract_Collection_ShouldMapAllItems()
    {
        // Arrange
        var dtos = new List<AllowedCityDto>
        {
            CreateTestDto(30.0),
            CreateTestDto(70.0),
            CreateTestDto(100.0)
        };

        // Act
        var contracts = dtos.ToContract();

        // Assert
        contracts.Should().HaveCount(3);
        contracts[0].ServiceRadiusKm.Should().Be(30);
        contracts[1].ServiceRadiusKm.Should().Be(70);
        contracts[2].ServiceRadiusKm.Should().Be(100);
    }

    [Fact]
    public void ToContract_EmptyCollection_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyDtos = Enumerable.Empty<AllowedCityDto>();

        // Act
        var contracts = emptyDtos.ToContract();

        // Assert
        contracts.Should().BeEmpty();
    }

    [Fact]
    public void ToContract_WithPreciseRadiusKm_ShouldRoundCorrectly()
    {
        // Arrange - 49.9999999 rounds to 50 (within 1e-6 tolerance)
        var dto = CreateTestDto(49.9999999);

        // Act
        var contract = dto.ToContract();

        // Assert
        contract.ServiceRadiusKm.Should().Be(50);
    }

    [Fact]
    public void ToContract_WithDecimalRadiusOutsideTolerance_ShouldThrowFormatException()
    {
        // Arrange - 50.5 has significant decimal part, exceeds 1e-6 tolerance
        var dto = CreateTestDto(50.5);

        // Act
        var act = () => dto.ToContract();

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("*raio de serviço*");
    }

    [Fact]
    public void ToContract_CountryAlwaysBrasil()
    {
        // Arrange
        var dto = CreateTestDto();

        // Act
        var contract = dto.ToContract();

        // Assert
        contract.Country.Should().Be("Brasil",
            "the module hardcodes Brasil as country since StateSigla is always Brazilian");
    }
}
