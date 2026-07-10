using MeAjudaAi.Modules.Locations.Application.ModuleApi;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.ModuleApi;

[Trait("Category", "Unit")]
public sealed class LocationsModuleApiTests
{
    private readonly Mock<ICepLookupService> _mockCepLookupService;
    private readonly Mock<IGeocodingService> _mockGeocodingService;
    private readonly Mock<IAllowedCityQueries> _mockAllowedCityQueries;
    private readonly Mock<ILogger<LocationsModuleApi>> _mockLogger;
    private readonly Mock<IStringLocalizer<Strings>> _mockLocalizer;
    private readonly LocationsModuleApi _sut;

    public LocationsModuleApiTests()
    {
        _mockCepLookupService = new Mock<ICepLookupService>();
        _mockGeocodingService = new Mock<IGeocodingService>();
        _mockAllowedCityQueries = new Mock<IAllowedCityQueries>();
        _mockLogger = new Mock<ILogger<LocationsModuleApi>>();
        _mockLocalizer = new Mock<IStringLocalizer<Strings>>();
        _mockLocalizer.Setup(x => x[It.IsAny<string>()]).Returns<string>(key => new LocalizedString(key, key));
        _mockLocalizer.Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()]).Returns<string, object[]>((key, args) => new LocalizedString(key, key));
        _sut = new LocationsModuleApi(_mockCepLookupService.Object, _mockGeocodingService.Object, _mockAllowedCityQueries.Object, _mockLogger.Object, _mockLocalizer.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturnLocation()
    {
        // Act
        var moduleName = _sut.ModuleName;

        // Assert
        moduleName.Should().Be("Locations");
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
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenServiceThrows_ShouldReturnFalse()
    {
        // Arrange
        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenOperationCancelled_ShouldRethrowOperationCanceledException()
    {
        // Arrange
        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAddressFromCepAsync_ShouldPropagateCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        _mockCepLookupService
            .Setup(x => x.LookupAsync(It.IsAny<Cep>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address?)null);

        // Act
        await _sut.GetAddressFromCepAsync("01310100", cts.Token);

        // Assert
        _mockCepLookupService.Verify(
            x => x.LookupAsync(It.IsAny<Cep>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetCoordinatesFromAddressAsync_ShouldPropagateCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        
        _mockGeocodingService
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
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
        result.Error!.Message.Should().Contain("InvalidCep");
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
        result.Error!.Message.Should().Contain("CepNotFound");
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
        result.Error!.Message.Should().Contain("AddressCannotBeEmpty");
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
        result.Error!.Message.Should().Contain("CoordinatesNotFoundForAddress");
    }

    [Fact]
    public async Task GetAllowedCityIdAsync_WhenCityExists_ShouldReturnId()
    {
        // Arrange
        var city = new AllowedCity("São Paulo", "SP", "admin");
        var cityId = city.Id;

        _mockAllowedCityQueries
            .Setup(x => x.GetByCityAndStateAsync("São Paulo", "SP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _sut.GetAllowedCityIdAsync("São Paulo", "SP");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cityId);
    }

    [Fact]
    public async Task GetAllowedCityIdAsync_WhenCityNotFound_ShouldReturnSuccessWithNull()
    {
        // Arrange
        _mockAllowedCityQueries
            .Setup(x => x.GetByCityAndStateAsync("Unknown", "XX", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var result = await _sut.GetAllowedCityIdAsync("Unknown", "XX");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetAllowedCityIdAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockAllowedCityQueries
            .Setup(x => x.GetByCityAndStateAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        await _sut.GetAllowedCityIdAsync("City", "ST", token);

        // Assert
        _mockAllowedCityQueries.Verify(
            x => x.GetByCityAndStateAsync("City", "ST", token),
            Times.Once);
    }

    [Fact]
    public async Task GetAllowedCityIdAsync_WhenExceptionThrown_ShouldReturnFailureWithoutExposingMessage()
    {
        // Arrange
        _mockAllowedCityQueries
            .Setup(x => x.GetByCityAndStateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal DB error"));

        // Act
        var result = await _sut.GetAllowedCityIdAsync("City", "ST");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().NotContain("Internal DB error");
        result.Error!.Message.Should().Contain("ErrorFetchingCityId");
    }
}
