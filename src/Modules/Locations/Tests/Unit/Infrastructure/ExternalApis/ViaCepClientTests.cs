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

public sealed class ViaCepClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly ViaCepClient _client;

    public ViaCepClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://viacep.com.br/")
        };

        _client = new ViaCepClient(_httpClient, NullLogger<ViaCepClient>.Instance);
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
        var viaCepResponse = new ViaCepResponse
        {
            Cep = "01001-000",
            Logradouro = "Praça da Sé",
            Complemento = "lado ímpar",
            Bairro = "Sé",
            Localidade = "São Paulo",
            Uf = "SP",
            Erro = false
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(viaCepResponse, SerializationDefaults.Default));

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Street.Should().Be("Praça da Sé");
        result.Neighborhood.Should().Be("Sé");
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
        result.Cep.Value.Should().Be("01001000");
    }

    [Fact]
    public async Task GetAddressAsync_WhenApiReturnsNotFound_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("99999999")!;
        var viaCepResponse = new ViaCepResponse { Erro = true };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(viaCepResponse, SerializationDefaults.Default));

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().BeNull();
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
    public async Task GetAddressAsync_WhenApiReturns404NotFound_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("00000000")!;
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
        var cep = Cep.Create("01001000");
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
        var cep = Cep.Create("01001000");
        var viaCepResponse = new ViaCepResponse
        {
            Cep = "01001-000",
            Logradouro = "Test",
            Bairro = "Test",
            Localidade = "Test",
            Uf = "SP",
            Erro = false
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(viaCepResponse, SerializationDefaults.Default));

        // Act
        await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        _mockHandler.LastRequestUri.Should().Contain("ws/01001000/json/");
    }

    // Mock HttpMessageHandler
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
