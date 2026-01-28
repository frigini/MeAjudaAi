using FluentAssertions;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.API.Mappers;

[Trait("Category", "Unit")]
[Trait("Layer", "API")]
[Trait("Component", "Mappers")]
public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_WithValidCreateAllowedCityRequest_ShouldMapToCreateAllowedCityCommand()
    {
        // Arrange
        var request = new CreateAllowedCityRequest(
            City: "Muriaé",
            State: "MG",
            IbgeCode: 3143906,
            IsActive: true
        );

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.CityName.Should().Be("Muriaé");
        command.StateSigla.Should().Be("MG");
        command.IbgeCode.Should().Be(3143906);
        command.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_WithNullIbgeCode_ShouldMapWithNullIbgeCode()
    {
        // Arrange
        var request = new CreateAllowedCityRequest(
            City: "Itaperuna",
            State: "RJ",
            IbgeCode: null,
            IsActive: true
        );

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.CityName.Should().Be("Itaperuna");
        command.StateSigla.Should().Be("RJ");
        command.IbgeCode.Should().BeNull();
        command.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_WithIsActiveFalse_ShouldMapWithIsActiveFalse()
    {
        // Arrange
        var request = new CreateAllowedCityRequest(
            City: "Linhares",
            State: "ES",
            IbgeCode: 3203205,
            IsActive: false
        );

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.CityName.Should().Be("Linhares");
        command.StateSigla.Should().Be("ES");
        command.IbgeCode.Should().Be(3203205);
        command.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToCommand_WithUpdateAllowedCityRequest_ShouldMapToUpdateCommand()
    {
        // Arrange
        var request = new UpdateAllowedCityRequest(
            City: "Belo Horizonte",
            State: "MG",
            IbgeCode: 3106200,
            IsActive: true
        );
        var id = Guid.NewGuid();

        // Act
        var command = request.ToCommand(id);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.CityName.Should().Be("Belo Horizonte");
        command.StateSigla.Should().Be("MG");
        command.IbgeCode.Should().Be(3106200);
        command.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ToCommand_WithUpdateRequestAndNullIbgeCode_ShouldMapCorrectly()
    {
        // Arrange
        var request = new UpdateAllowedCityRequest(
            City: "Rio de Janeiro",
            State: "RJ",
            IbgeCode: null,
            IsActive: false
        );
        var id = Guid.NewGuid();

        // Act
        var command = request.ToCommand(id);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.CityName.Should().Be("Rio de Janeiro");
        command.StateSigla.Should().Be("RJ");
        command.IbgeCode.Should().BeNull();
        command.IsActive.Should().BeFalse();
    }

    [Fact]
    public void ToDeleteCommand_WithValidGuid_ShouldMapToDeleteAllowedCityCommand()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = id.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
    }

    [Fact]
    public void ToDeleteCommand_WithEmptyGuid_ShouldMapToDeleteCommand()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var command = id.ToDeleteCommand();

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(Guid.Empty);
    }

    [Theory]
    [InlineData("São Paulo", "SP", 3550308)]
    [InlineData("Vitória", "ES", 3205309)]
    [InlineData("Niterói", "RJ", 3303302)]
    public void ToCommand_WithDifferentCities_ShouldMapCorrectly(string cityName, string state, int ibgeCode)
    {
        // Arrange
        var request = new CreateAllowedCityRequest(
            City: cityName,
            State: state,
            IbgeCode: ibgeCode,
            IsActive: true
        );

        // Act
        var command = request.ToCommand();

        // Assert
        command.Should().NotBeNull();
        command.CityName.Should().Be(cityName);
        command.StateSigla.Should().Be(state);
        command.IbgeCode.Should().Be(ibgeCode);
        command.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("Curitiba", "PR", 4106902, true)]
    [InlineData("Porto Alegre", "RS", 4314902, false)]
    public void ToCommand_WithUpdateRequestAndDifferentStates_ShouldMapCorrectly(
        string cityName,
        string state,
        int ibgeCode,
        bool isActive)
    {
        // Arrange
        var request = new UpdateAllowedCityRequest(
            City: cityName,
            State: state,
            IbgeCode: ibgeCode,
            IsActive: isActive
        );
        var id = Guid.NewGuid();

        // Act
        var command = request.ToCommand(id);

        // Assert
        command.Should().NotBeNull();
        command.Id.Should().Be(id);
        command.CityName.Should().Be(cityName);
        command.StateSigla.Should().Be(state);
        command.IbgeCode.Should().Be(ibgeCode);
        command.IsActive.Should().Be(isActive);
    }
}
