using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Services;

public sealed class GeographicValidationServiceTests
{
    private readonly Mock<IIbgeService> _mockIbgeService;
    private readonly Mock<ILogger<GeographicValidationService>> _mockLogger;
    private readonly GeographicValidationService _service;

    public GeographicValidationServiceTests()
    {
        _mockIbgeService = new Mock<IIbgeService>();
        _mockLogger = new Mock<ILogger<GeographicValidationService>>();
        _service = new GeographicValidationService(_mockIbgeService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ValidateCityAsync_ShouldDelegateToIbgeService()
    {
        // Arrange
        var cityName = "Muriaé";
        var stateSigla = "MG";
        var allowedCities = new List<string> { "Muriaé", "Itaperuna", "Linhares" };
        var cancellationToken = CancellationToken.None;

        _mockIbgeService
            .Setup(x => x.ValidateCityInAllowedRegionsAsync(
                cityName,
                stateSigla,
                allowedCities,
                cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ValidateCityAsync(cityName, stateSigla, allowedCities, cancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockIbgeService.Verify(
            x => x.ValidateCityInAllowedRegionsAsync(cityName, stateSigla, allowedCities, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ValidateCityAsync_WhenCityNotAllowed_ShouldReturnFalse()
    {
        // Arrange
        var cityName = "São Paulo";
        var stateSigla = "SP";
        var allowedCities = new List<string> { "Muriaé" };
        var cancellationToken = CancellationToken.None;

        _mockIbgeService
            .Setup(x => x.ValidateCityInAllowedRegionsAsync(
                cityName,
                stateSigla,
                allowedCities,
                cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ValidateCityAsync(cityName, stateSigla, allowedCities, cancellationToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCityAsync_WithNullStateSigla_ShouldPassNullToIbgeService()
    {
        // Arrange
        var cityName = "Muriaé";
        string? stateSigla = null;
        var allowedCities = new List<string> { "Muriaé" };
        var cancellationToken = CancellationToken.None;

        _mockIbgeService
            .Setup(x => x.ValidateCityInAllowedRegionsAsync(
                cityName,
                stateSigla,
                allowedCities,
                cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ValidateCityAsync(cityName, stateSigla, allowedCities, cancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockIbgeService.Verify(
            x => x.ValidateCityInAllowedRegionsAsync(cityName, null, allowedCities, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ValidateCityAsync_WhenIbgeServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var cityName = "Muriaé";
        var stateSigla = "MG";
        var allowedCities = new List<string> { "Muriaé" };
        var cancellationToken = CancellationToken.None;
        var exception = new HttpRequestException("IBGE API unavailable");

        _mockIbgeService
            .Setup(x => x.ValidateCityInAllowedRegionsAsync(
                cityName,
                stateSigla,
                allowedCities,
                cancellationToken))
            .ThrowsAsync(exception);

        // Act
        Func<Task> act = async () => await _service.ValidateCityAsync(cityName, stateSigla, allowedCities, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("IBGE API unavailable");
    }
}
