using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.ModuleApi;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
public sealed class LocationsModuleApiTests
{
    private readonly Mock<ICepLookupService> _mockCepLookupService;
    private readonly Mock<IGeocodingService> _mockGeocodingService;
    private readonly Mock<ILogger<LocationsModuleApi>> _mockLogger;
    private readonly LocationsModuleApi _sut;

    public LocationsModuleApiTests()
    {
        _mockCepLookupService = new Mock<ICepLookupService>();
        _mockGeocodingService = new Mock<IGeocodingService>();
        _mockLogger = new Mock<ILogger<LocationsModuleApi>>();
        _sut = new LocationsModuleApi(_mockCepLookupService.Object, _mockGeocodingService.Object, _mockLogger.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturnLocation()
    {
        // Act
        var moduleName = _sut.ModuleName;

        // Assert
        moduleName.Should().Be("Location");
    }

    [Fact]
    public void ApiVersion_ShouldReturn1_0()
    {
        // Act
        var version = _sut.ApiVersion;

        // Assert
        version.Should().Be("1.0");
    }

    [Fact]
    public async Task IsAvailableAsync_WhenServiceWorks_ShouldReturnTrue()
    {
        // Arrange
        var cep = Cep.Create("01310100")!;
        var address = Address.Create(
            cep,
            "Avenida Paulista",
            "Bela Vista",
            "São Paulo",
            "SP",
            null,
            new GeoPoint(-23.561414, -46.656559));

        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _sut.IsAvailableAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cancelled*");
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WhenCancelled_ShouldPropagateToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .Callback<Cep, CancellationToken>((_, token) =>
            {
                // Verificar que o token foi propagado
                token.Should().Be(cts.Token);
            })
            .ReturnsAsync((Address?)null);

        // Act
        await _sut.GetAddressFromCepAsync("01310100", cts.Token);

        // Assert
        _mockCepLookupService.Verify(
            x => x.LookupAsync(It.IsAny<Cep>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WhenCancelled_ShouldPropagateToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        _mockGeocodingService
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((_, token) =>
            {
                // Verificar que o token foi propagado
                token.Should().Be(cts.Token);
            })
            .ReturnsAsync((GeoPoint?)null);

        // Act
        await _sut.GetCoordinatesFromAddressAsync("Test Address", cts.Token);

        // Assert
        _mockGeocodingService.Verify(
            x => x.GetCoordinatesAsync(It.IsAny<string>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WithValidCep_ShouldReturnSuccess()
    {
        // Arrange
        var cep = Cep.Create("01310100")!;
        var address = Address.Create(
            cep,
            "Avenida Paulista",
            "Bela Vista",
            "São Paulo",
            "SP",
            null,
            new GeoPoint(-23.561414, -46.656559));

        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.Is<Cep>(c => c.Value == "01310100"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        // Act
        var result = await _sut.GetAddressFromCepAsync("01310100");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Cep.Should().Be("01310-100");
        result.Value.Street.Should().Be("Avenida Paulista");
        result.Value.City.Should().Be("São Paulo");
        result.Value.State.Should().Be("SP");
        result.Value.Coordinates.Should().NotBeNull();
        result.Value.Coordinates!.Latitude.Should().Be(-23.561414);
        result.Value.Coordinates.Longitude.Should().Be(-46.656559);
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WithInvalidCep_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.GetAddressFromCepAsync("invalid");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("inválido");
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WhenCepNotFound_ShouldReturnFailure()
    {
        // Arrange
        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        // Act
        var result = await _sut.GetAddressFromCepAsync("99999999");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task GetAddressFromCepAsync_WithoutCoordinates_ShouldReturnSuccessWithNullCoordinates()
    {
        // Arrange
        var cep = Cep.Create("01310100")!;
        var address = Address.Create(
            cep,
            "Avenida Paulista",
            "Bela Vista",
            "São Paulo",
            "SP");

        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(address);

        // Act
        var result = await _sut.GetAddressFromCepAsync("01310100");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Coordinates.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WithValidAddress_ShouldReturnSuccess()
    {
        // Arrange
        var geoPoint = new GeoPoint(-23.561414, -46.656559);
        _mockGeocodingService
            .Setup(x => x.GetCoordinatesAsync("Avenida Paulista, São Paulo, SP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(geoPoint);

        // Act
        var result = await _sut.GetCoordinatesFromAddressAsync("Avenida Paulista, São Paulo, SP");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Latitude.Should().Be(-23.561414);
        result.Value.Longitude.Should().Be(-46.656559);
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WithEmptyAddress_ShouldReturnFailure()
    {
        // Act
        var result = await _sut.GetCoordinatesFromAddressAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("não pode ser vazio");
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_WhenCoordinatesNotFound_ShouldReturnFailure()
    {
        // Arrange
        _mockGeocodingService
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoPoint?)null);

        // Act
        var result = await _sut.GetCoordinatesFromAddressAsync("Endereço inexistente");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("não encontradas");
    }
}
