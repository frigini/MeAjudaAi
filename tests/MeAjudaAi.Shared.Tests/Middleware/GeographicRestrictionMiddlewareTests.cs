using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.ApiService.Middlewares;
using MeAjudaAi.ApiService.Options;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace MeAjudaAi.Shared.Tests.Middleware;

public class GeographicRestrictionMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<GeographicRestrictionMiddleware>> _loggerMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly DefaultHttpContext _httpContext;

    public GeographicRestrictionMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _nextMock.Setup(d => d(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
        _loggerMock = new Mock<ILogger<GeographicRestrictionMiddleware>>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_WhenDisabled_ShouldCallNext()
    {
        // Arrange
        SetupFeatureFlag(false);
        var options = CreateOptions();
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/ready")]
    [InlineData("/swagger")]
    [InlineData("/swagger/index.html")]
    [InlineData("/_framework/blazor.boot.json")]
    public async Task InvokeAsync_WhenInternalEndpoint_ShouldCallNext(string path)
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = path;

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNoLocationDetected_ShouldCallNext_FailOpen()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not determine user location")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Muriaé", "MG")]
    [InlineData("muriaé", "mg")] // Case insensitive
    [InlineData("MURIAÉ", "MG")]
    [InlineData("Itaperuna", "RJ")]
    [InlineData("Linhares", "ES")]
    public async Task InvokeAsync_WhenAllowedCity_ShouldCallNext(string city, string state)
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = city;
        _httpContext.Request.Headers["X-User-State"] = state;

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Theory]
    [InlineData("MG")]
    [InlineData("mg")] // Case insensitive
    [InlineData("RJ")]
    [InlineData("ES")]
    public async Task InvokeAsync_WhenAllowedState_ShouldCallNext(string state)
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-State"] = state;

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Theory]
    [InlineData("Muriaé|MG")]
    [InlineData("Itaperuna|RJ")]
    [InlineData("Linhares|ES")]
    public async Task InvokeAsync_WhenLocationHeaderFormat_ShouldCallNext(string location)
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-Location"] = location;

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Theory]
    [InlineData("São Paulo", "SP")]
    [InlineData("Rio de Janeiro", "RJ")]
    [InlineData("Curitiba", "PR")]
    [InlineData("Fortaleza", "CE")]
    public async Task InvokeAsync_WhenBlockedCity_ShouldReturn451(string city, string state)
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-Location"] = $"{city}|{state}"; // Use new format

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(451); // Unavailable For Legal Reasons
        _httpContext.Response.ContentType.Should().Be("application/json");

        // Verificar resposta JSON
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();

        responseBody.Should().NotBeNullOrEmpty();

        var response = JsonSerializer.Deserialize<JsonElement>(responseBody);
        response.GetProperty("error").GetString().Should().Be("geographic_restriction");
        // The response uses "detail" property, not "message"
        response.GetProperty("detail").GetString().Should().Contain("Muriaé");
        response.GetProperty("allowedCities").GetArrayLength().Should().Be(3);

        // Verify actual allowed city names match configuration (allowedCities are objects with name/state properties)
        var allowedCities = response.GetProperty("allowedCities").EnumerateArray()
            .Select(e => e.GetProperty("name").GetString())
            .ToList();
        allowedCities.Should().Contain(new[] { "Muriaé", "Itaperuna", "Linhares" });

        response.GetProperty("yourLocation").GetProperty("city").GetString().Should().Be(city);
        response.GetProperty("yourLocation").GetProperty("state").GetString().Should().Be(state);
    }

    [Fact]
    public async Task InvokeAsync_WhenBlocked_ShouldLogWarning()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "São Paulo";
        _httpContext.Request.Headers["X-User-State"] = "SP";
        _httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Request blocked from São Paulo/SP")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenXUserLocationHeader_ShouldParseCityAndState()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-Location"] = "Muriaé|MG";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenMultipleHeaders_ShouldPrioritizeXUserLocation()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-Location"] = "Muriaé|MG";
        _httpContext.Request.Headers["X-User-City"] = "São Paulo"; // Should be ignored
        _httpContext.Request.Headers["X-User-State"] = "SP"; // Should be ignored

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert (Muriaé is allowed, São Paulo is not)
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Theory]
    [InlineData("InvalidFormat")]
    [InlineData("City")]
    [InlineData("")]
    public async Task InvokeAsync_WhenInvalidLocationHeader_ShouldFallbackToSeparateHeaders(string invalidLocation)
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object);
        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-Location"] = invalidLocation;
        _httpContext.Request.Headers["X-User-City"] = "Muriaé";
        _httpContext.Request.Headers["X-User-State"] = "MG";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    private static IOptionsMonitor<GeographicRestrictionOptions> CreateOptions()
    {
        var mock = new Mock<IOptionsMonitor<GeographicRestrictionOptions>>();
        mock.Setup(m => m.CurrentValue).Returns(new GeographicRestrictionOptions
        {
            AllowedStates = ["MG", "RJ", "ES"],
            AllowedCities = ["Muriaé", "Itaperuna", "Linhares"],
            BlockedMessage = "Serviço indisponível na sua região. Disponível apenas em: {allowedRegions}"
        });
        return mock.Object;
    }

    #region IBGE Validation Tests

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_WhenCityAllowed_ShouldCallNext()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                "Muriaé", "MG", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "Muriaé";
        _httpContext.Request.Headers["X-User-State"] = "MG";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        geographicValidationMock.Verify(
            x => x.ValidateCityAsync("Muriaé", "MG", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_WhenCityBlocked_ShouldReturn451()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                "São Paulo", "SP", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "São Paulo";
        _httpContext.Request.Headers["X-User-State"] = "SP";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(451);
        geographicValidationMock.Verify(
            x => x.ValidateCityAsync("São Paulo", "SP", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_WhenServiceThrowsException_ShouldFallbackToSimple()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("IBGE API down"));

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "Muriaé";
        _httpContext.Request.Headers["X-User-State"] = "MG";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert (fallback to simple validation - Muriaé is in AllowedCities)
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error validating with IBGE")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_WhenServiceUnavailable_ShouldFallbackToSimple()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        // Null service simula serviço não disponível
        var middleware = new GeographicRestrictionMiddleware(
            _nextMock.Object,
            _loggerMock.Object,
            options,
            _featureManagerMock.Object,
            geographicValidationService: null);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "Muriaé";
        _httpContext.Request.Headers["X-User-State"] = "MG";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert (fallback to simple validation - Muriaé is in AllowedCities)
        _nextMock.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_CaseInsensitive_ShouldWork()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                "muriaé", // lowercase
                "mg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "muriaé";
        _httpContext.Request.Headers["X-User-State"] = "mg";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        geographicValidationMock.Verify(
            x => x.ValidateCityAsync("muriaé", "mg", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_WhenStateNotProvided_ShouldPassNull()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                "Muriaé", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "Muriaé";
        // No state header

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        geographicValidationMock.Verify(
            x => x.ValidateCityAsync("Muriaé", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_LogsIbgeUsage()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "Muriaé";
        _httpContext.Request.Headers["X-User-State"] = "MG";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Debug || l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validating city") || v.ToString()!.Contains("IBGE validation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InvokeAsync_WithIbgeValidation_WhenBothIbgeAndSimpleAgree_ShouldAllow()
    {
        // Arrange
        var options = CreateOptions();
        SetupFeatureFlag(true);
        var geographicValidationMock = new Mock<IGeographicValidationService>();
        geographicValidationMock
            .Setup(x => x.ValidateCityAsync(
                "Itaperuna", "RJ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var middleware = new GeographicRestrictionMiddleware(_nextMock.Object, _loggerMock.Object, options, _featureManagerMock.Object, geographicValidationMock.Object);

        _httpContext.Request.Path = "/api/providers";
        _httpContext.Request.Headers["X-User-City"] = "Itaperuna";
        _httpContext.Request.Headers["X-User-State"] = "RJ";

        // Act
        await middleware.InvokeAsync(_httpContext);

        // Assert (both IBGE and simple validation should agree - Itaperuna is in AllowedCities)
        _nextMock.Verify(next => next(_httpContext), Times.Once);
        geographicValidationMock.Verify(
            x => x.ValidateCityAsync("Itaperuna", "RJ", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private void SetupFeatureFlag(bool enabled)
    {
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.GeographicRestriction))
            .ReturnsAsync(enabled);
    }

    #endregion
}


