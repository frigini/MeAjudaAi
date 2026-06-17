using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Layer", "API")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_CreateAllowedCityRequest_ShouldMapAllProperties()
    {
        // Arrange
        var request = new CreateAllowedCityRequest(
            CityName: "Muriaé",
            StateSigla: "MG",
            IbgeCode: 3143906,
            Latitude: -21.13,
            Longitude: -42.37,
            ServiceRadiusKm: 50.0,
            IsActive: true);

        // Act
        var command = request.ToCommand();

        // Assert
        command.CityName.Should().Be("Muriaé");
        command.StateSigla.Should().Be("MG");
        command.IbgeCode.Should().Be(3143906);
        command.Latitude.Should().Be(-21.13);
        command.Longitude.Should().Be(-42.37);
        command.ServiceRadiusKm.Should().Be(50.0);
        command.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_CreateAllowedCityRequestDto_ShouldMapContractProperties()
    {
        // Arrange
        var dto = new MeAjudaAi.Contracts.Modules.Locations.DTOs.CreateAllowedCityRequestDto(
            City: "Linhares",
            State: "ES",
            Country: "Brasil",
            Latitude: -19.39,
            Longitude: -40.07,
            ServiceRadiusKm: 80,
            IsActive: true);

        // Act
        var command = dto.ToCommand();

        // Assert
        command.CityName.Should().Be("Linhares");
        command.StateSigla.Should().Be("ES");
        command.IbgeCode.Should().BeNull("contract DTO does not carry IBGE code");
        command.Latitude.Should().Be(-19.39);
        command.Longitude.Should().Be(-40.07);
        command.ServiceRadiusKm.Should().Be(80);
        command.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_UpdateAllowedCityRequest_ShouldMapAllPropertiesIncludingId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = new UpdateAllowedCityRequest(
            CityName: "Itaperuna",
            StateSigla: "RJ",
            IbgeCode: 3302504,
            Latitude: -21.21,
            Longitude: -41.89,
            ServiceRadiusKm: 60.0,
            IsActive: false);

        // Act
        var command = request.ToCommand(id);

        // Assert
        command.Id.Should().Be(id);
        command.CityName.Should().Be("Itaperuna");
        command.StateSigla.Should().Be("RJ");
        command.IbgeCode.Should().Be(3302504);
        command.Latitude.Should().Be(-21.21);
        command.Longitude.Should().Be(-41.89);
        command.ServiceRadiusKm.Should().Be(60.0);
        command.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToDeleteCommand_ShouldMapIdToCommand()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToDeleteCommand();

        // Assert
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToDeleteCommand_WithEmptyGuid_ShouldMapEmptyGuid()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var command = emptyId.ToDeleteCommand();

        // Assert
        command.Id.Should().Be(Guid.Empty);
    }
}
