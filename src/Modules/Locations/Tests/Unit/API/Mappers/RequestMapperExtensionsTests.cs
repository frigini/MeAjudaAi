using System;
using FluentAssertions;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.API.Mappers;

public class RequestMapperExtensionsTests
{
    [Fact]
    public void ToCommand_FromInternalRequest_ShouldMapCorrectly()
    {
        // Arrange
        var request = new CreateAllowedCityRequest(
            CityName: "Vitoria",
            StateSigla: "ES",
            IbgeCode: 123456,
            Latitude: -20.0,
            Longitude: -40.0,
            ServiceRadiusKm: 50,
            IsActive: true
        );

        // Act
        var command = request.ToCommand();

        // Assert
        command.CityName.Should().Be(request.CityName);
        command.StateSigla.Should().Be(request.StateSigla);
        command.IbgeCode.Should().Be(request.IbgeCode);
        command.Latitude.Should().Be(request.Latitude);
        command.Longitude.Should().Be(request.Longitude);
        command.ServiceRadiusKm.Should().Be(request.ServiceRadiusKm);
        command.IsActive.Should().Be(request.IsActive);
    }

    [Fact]
    public void ToCommand_FromContractRequestDto_ShouldMapCorrectly()
    {
        // Arrange
        var requestDto = new CreateAllowedCityRequestDto(
            City: "Serra",
            State: "ES",
            Country: "Brasil",
            Latitude: -20.1,
            Longitude: -40.2,
            ServiceRadiusKm: 30,
            IsActive: true
        );

        // Act
        var command = requestDto.ToCommand();

        // Assert
        command.CityName.Should().Be(requestDto.City);
        command.StateSigla.Should().Be(requestDto.State);
        command.IbgeCode.Should().BeNull(); // DTO doesn't have IBGE code
        command.Latitude.Should().Be(requestDto.Latitude);
        command.Longitude.Should().Be(requestDto.Longitude);
        command.ServiceRadiusKm.Should().Be(requestDto.ServiceRadiusKm);
        command.IsActive.Should().Be(requestDto.IsActive);
    }
}
