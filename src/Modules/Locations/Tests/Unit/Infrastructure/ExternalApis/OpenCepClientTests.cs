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

public sealed class OpenCepClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly OpenCepClient _client;

    public OpenCepClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://opencep.com/")
        };

        _client = new OpenCepClient(_httpClient, NullLogger<OpenCepClient>.Instance);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    [Fact]
    public async Task GetAddressAsync_WithValidCep_ShouldReturnAddress()
    {
        // Arrange
        var cep = Cep.Create("01001000");
        var openCepResponse = new OpenCepResponse
        {
            Cep = "01001-000",
            Logradouro = "Praça da Sé",
            Complemento = "lado ímpar",
            Bairro = "Sé",
            Localidade = "São Paulo",
            Uf = "SP"
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(openCepResponse, SerializationDefaults.Default));

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Street.Should().Be("Praça da Sé");
        result.Neighborhood.Should().Be("Sé");
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
    }

    [Fact]
    public async Task GetAddressAsync_WhenApiReturnsError_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("01001000");
        _mockHandler.SetResponse(HttpStatusCode.InternalServerError, "");

        // Act
        var result = await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressAsync_WhenApiThrowsException_ShouldReturnNull()
    {
        // Arrange
        var cep = Cep.Create("01001000");
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
        var openCepResponse = new OpenCepResponse
        {
            Cep = "01001-000",
            Logradouro = "Test",
            Bairro = "Test",
            Localidade = "Test",
            Uf = "SP"
        };

        _mockHandler.SetResponse(
            HttpStatusCode.OK,
            JsonSerializer.Serialize(openCepResponse, SerializationDefaults.Default));

        // Act
        await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        _mockHandler.LastRequestUri.Should().Contain("v1/01001000");
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
