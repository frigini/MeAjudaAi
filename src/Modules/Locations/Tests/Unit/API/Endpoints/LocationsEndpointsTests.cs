using FluentAssertions;
using MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.API.Endpoints;

public class LocationsEndpointsTests
{
    private readonly Mock<IGeocodingService> _geocodingServiceMock;

    public LocationsEndpointsTests()
    {
        _geocodingServiceMock = new Mock<IGeocodingService>();
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnResults_WhenQueryIsValid()
    {
        // Arrange
        var query = "Muriaé";
        var candidates = new List<LocationCandidate> { 
            new LocationCandidate("Muriaé, MG", "Muriaé", "MG", "BR", -21.1306, -42.3664) 
        };
        _geocodingServiceMock.Setup(x => x.SearchAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);

        // Act
        var result = await CallSearchAsync(query);

        // Assert
        result.Should().BeOfType<Ok<List<LocationCandidate>>>();
        var okResult = (Ok<List<LocationCandidate>>)result;
        okResult.Value.Should().BeEquivalentTo(candidates);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenQueryTooShort()
    {
        // Arrange
        var emptyList = new List<LocationCandidate>();

        // Act
        var result = await CallSearchAsync("Mu");

        // Assert
        result.Should().BeOfType<Ok<LocationCandidate[]>>(); // SearchLocationsEndpoint returns Array.Empty<LocationCandidate>()
        var okResult = (Ok<LocationCandidate[]>)result;
        okResult.Value.Should().BeEmpty();
    }

    private async Task<IResult> CallSearchAsync(string query)
    {
        var method = typeof(SearchLocationsEndpoint).GetMethod("SearchAsync", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        return await (Task<IResult>)method!.Invoke(null, new object[] 
        { 
            query,
            _geocodingServiceMock.Object, 
            CancellationToken.None 
        })!;
    }
}
