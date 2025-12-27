using System.Net;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.ExternalApis;

public sealed class NominatimClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly FakeTimeProvider _timeProvider;
    private readonly NominatimClient _client;

    public NominatimClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://nominatim.openstreetmap.org/")
        };

        _timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        _client = new NominatimClient(_httpClient, NullLogger<NominatimClient>.Instance, _timeProvider);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _httpClient?.Dispose();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WithValidAddress_ShouldReturnCoordinates()
    {
        // Arrange
        var address = "Praça da Sé, São Paulo, SP";
        var nominatimResponses = new[]
        {
            new NominatimResponse
            {
                Lat = "-23.5505",
                Lon = "-46.6333",
                DisplayName = "Praça da Sé, São Paulo, SP, Brasil"
            }
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(nominatimResponses, SerializationDefaults.Default));

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().BeApproximately(-23.5505, 0.0001);
        result.Longitude.Should().BeApproximately(-46.6333, 0.0001);
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenAddressIsNull_ShouldReturnNull()
    {
        // Act
        var result = await _client.GetCoordinatesAsync(null!, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenAddressIsEmpty_ShouldReturnNull()
    {
        // Act
        var result = await _client.GetCoordinatesAsync(string.Empty, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenApiReturnsEmptyArray_ShouldReturnNull()
    {
        // Arrange
        var address = "Endereço Inexistente, Brasil";
        _mockHandler.SetResponse(HttpStatusCode.OK, "[]");

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenApiReturnsError_ShouldReturnNull()
    {
        // Arrange
        var address = "Test Address";
        _mockHandler.SetResponse(HttpStatusCode.InternalServerError, "");

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenApiThrowsException_ShouldReturnNull()
    {
        // Arrange
        var address = "Test Address";
        _mockHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenResponseIsNull_ShouldReturnNull()
    {
        // Arrange
        var address = "Test Address";
        _mockHandler.SetResponse(HttpStatusCode.OK, "null");

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenLatitudeIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var address = "Test Address";
        var nominatimResponses = new[]
        {
            new NominatimResponse
            {
                Lat = "invalid",
                Lon = "-46.6333",
                DisplayName = "Test"
            }
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(nominatimResponses, SerializationDefaults.Default));

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_WhenLongitudeIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var address = "Test Address";
        var nominatimResponses = new[]
        {
            new NominatimResponse
            {
                Lat = "-23.5505",
                Lon = "invalid",
                DisplayName = "Test"
            }
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(nominatimResponses, SerializationDefaults.Default));

        // Act
        var result = await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCoordinatesAsync_ShouldUrlEncodeAddress()
    {
        // Arrange
        var address = "Praça da Sé, São Paulo";
        var nominatimResponses = new[]
        {
            new NominatimResponse
            {
                Lat = "-23.5505",
                Lon = "-46.6333",
                DisplayName = "Test"
            }
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(nominatimResponses, SerializationDefaults.Default));

        // Act
        await _client.GetCoordinatesAsync(address, CancellationToken.None);

        // Assert
        _mockHandler.LastRequestUri.Should().Contain("search?q=");
        _mockHandler.LastRequestUri.Should().Contain("format=json");
        _mockHandler.LastRequestUri.Should().Contain("limit=1");
        _mockHandler.LastRequestUri.Should().Contain("countrycodes=br");
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage? _responseMessage;
        private Exception? _exception;
        public string? LastRequestUri { get; private set; }

        public void SetResponse(HttpStatusCode statusCode, string content)
        {
            _responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            _exception = null;
        }

        public void SetException(Exception exception)
        {
            _exception = exception;
            _responseMessage = null;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri?.ToString();

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_responseMessage ?? new HttpResponseMessage(HttpStatusCode.InternalServerError));
        }
    }
}
