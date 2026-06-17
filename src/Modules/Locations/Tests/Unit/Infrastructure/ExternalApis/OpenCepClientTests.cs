using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.ExternalApis;

public sealed class OpenCepClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly OpenCepClient _client;

    public OpenCepClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler.GetHandler())
        {
            BaseAddress = new Uri("https://opencep.com/")
        };

        _client = new OpenCepClient(_httpClient, NullLogger<OpenCepClient>.Instance, new SystemTextJsonSerializer(SerializationDefaults.Api));
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    [Fact]
    public async Task GetAddressAsync_WithValidCep_ShouldReturnAddress()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
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
            System.Text.Json.JsonSerializer.Serialize(openCepResponse, SerializationDefaults.Default));

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
    public async Task GetAddressAsync_WhenApiThrowsException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cep = Cep.Create("01001000");
        _mockHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var act = async () => await _client.GetAddressAsync(cep!, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.And.InnerException.Should().BeOfType<HttpRequestException>();
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
            System.Text.Json.JsonSerializer.Serialize(openCepResponse, SerializationDefaults.Default));

        // Act
        await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        _mockHandler.LastRequestUri.Should().Contain("v1/01001000");
    }

}
