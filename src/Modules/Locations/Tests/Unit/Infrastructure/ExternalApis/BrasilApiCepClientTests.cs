using System.Net;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.ExternalApis;

public sealed class BrasilApiCepClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly BrasilApiCepClient _client;

    public BrasilApiCepClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://brasilapi.com.br/")
        };

        _client = new BrasilApiCepClient(_httpClient, NullLogger<BrasilApiCepClient>.Instance);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _mockHandler?.Dispose();
    }

    [Fact]
    public async Task GetAddressAsync_WithValidCep_ShouldReturnAddress()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
        var brasilApiResponse = new BrasilApiCepResponse
        {
            Cep = "01001000",
            State = "SP",
            City = "São Paulo",
            Neighborhood = "Sé",
            Street = "Praça da Sé"
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(brasilApiResponse, SerializationDefaults.Default));

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Street.Should().Be("Praça da Sé");
        result.Neighborhood.Should().Be("Sé");
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
        result.Cep.Should().Be(cep);
    }

    [Fact]
    public async Task GetAddressAsync_WhenApiReturnsInternalServerError_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
        _mockHandler.SetResponse(HttpStatusCode.InternalServerError, "");

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressAsync_WhenApiReturnsNotFound_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("99999999")!;
        _mockHandler.SetResponse(HttpStatusCode.NotFound, "");

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressAsync_WhenHttpClientThrowsException_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
        _mockHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressAsync_WhenResponseIsNull_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
        _mockHandler.SetResponse(HttpStatusCode.OK, "null");

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressAsync_ShouldUseCorrectUrl()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
        var brasilApiResponse = new BrasilApiCepResponse
        {
            Cep = "01001000",
            State = "SP",
            City = "Test",
            Neighborhood = "Test",
            Street = "Test"
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(brasilApiResponse, SerializationDefaults.Default));

        // Act
        await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        _mockHandler.LastRequestUri.Should().Contain("api/cep/v2/01001000");
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler, IDisposable
    {
        private HttpResponseMessage? _responseMessage;
        private Exception? _exception;
        public string? LastRequestUri { get; private set; }

        public void SetResponse(HttpStatusCode statusCode, string content)
        {
            _responseMessage?.Dispose();
            _responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            };
            _exception = null;
        }

        public void SetException(Exception exception)
        {
            _responseMessage?.Dispose();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _responseMessage?.Dispose();
                _responseMessage = null;
            }
            base.Dispose(disposing);
        }
    }
}
