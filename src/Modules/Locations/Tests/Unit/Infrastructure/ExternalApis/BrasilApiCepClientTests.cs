using MeAjudaAi.Modules.Locations.Domain.ValueObjects;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Responses;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.ExternalApis;

public sealed class BrasilApiCepClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly BrasilApiCepClient _client;

    public BrasilApiCepClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler.GetHandler())
        {
            BaseAddress = new Uri("https://brasilapi.com.br/")
        };

        _client = new BrasilApiCepClient(_httpClient, NullLogger<BrasilApiCepClient>.Instance, new SystemTextJsonSerializer(SerializationDefaults.Api));
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
            System.Text.Json.JsonSerializer.Serialize(brasilApiResponse, SerializationDefaults.Default));

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
    public async Task GetAddressAsync_WhenHttpClientThrowsException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cep = Cep.Create("01001000")!;
        _mockHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var act = async () => await _client.GetAddressAsync(cep, CancellationToken.None);

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
            System.Text.Json.JsonSerializer.Serialize(brasilApiResponse, SerializationDefaults.Default));

        // Act
        await _client.GetAddressAsync(cep, CancellationToken.None);

        // Assert
        _mockHandler.LastRequestUri.Should().Contain("api/cep/v2/01001000");
    }

}
