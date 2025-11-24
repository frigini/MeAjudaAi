using System.Net;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using MeAjudaAi.Shared.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.ExternalApis;

public sealed class IbgeClientTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly IbgeClient _client;

    public IbgeClientTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler)
        {
            BaseAddress = new Uri("https://servicodados.ibge.gov.br/api/v1/localidades/")
        };

        _client = new IbgeClient(_httpClient, NullLogger<IbgeClient>.Instance);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    [Theory]
    [InlineData("Muriaé")]
    [InlineData("São Paulo")]
    [InlineData("Rio de Janeiro")]
    [InlineData("Itaperuna")]
    public async Task GetMunicipioByNameAsync_WithValidCity_ShouldUseQueryParameter(string input)
    {
        // Arrange
        var municipio = CreateMockMunicipio(1, input);
        _mockHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { municipio }, SerializationDefaults.Default));

        // Act
        var result = await _client.GetMunicipioByNameAsync(input);

        // Assert
        result.Should().NotBeNull();
        // IbgeClient uses lowercase for API queries
        var uriString = _mockHandler.LastRequestUri!.ToString();
        uriString.Should().Contain("municipios?nome=");
        uriString.Should().Contain(input.ToLowerInvariant()); // Query string is lowercase
    }

    [Fact]
    public async Task GetMunicipioByNameAsync_WhenApiReturnsSuccess_ShouldReturnMunicipio()
    {
        // Arrange
        var expectedMunicipio = CreateMockMunicipio(3104502, "Muriaé");
        _mockHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { expectedMunicipio }, SerializationDefaults.Default));

        // Act
        var result = await _client.GetMunicipioByNameAsync("Muriaé");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(3104502);
        result.Nome.Should().Be("Muriaé");
    }

    [Fact]
    public async Task GetMunicipioByNameAsync_WhenApiReturnsEmptyArray_ShouldReturnNull()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.OK, "[]");

        // Act
        var result = await _client.GetMunicipioByNameAsync("CidadeInexistente");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMunicipioByNameAsync_WhenApiReturnsError_ShouldThrowHttpRequestException()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.NotFound, "");

        // Act
        var act = async () => await _client.GetMunicipioByNameAsync("Muriaé");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetMunicipioByNameAsync_WhenApiThrowsException_ShouldPropagateException()
    {
        // Arrange
        _mockHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var act = async () => await _client.GetMunicipioByNameAsync("Muriaé");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Network error");
    }

    [Theory]
    [InlineData("MG")]
    [InlineData("RJ")]
    [InlineData("ES")]
    public async Task GetMunicipiosByUFAsync_WhenApiReturnsSuccess_ShouldReturnList(string uf)
    {
        // Arrange
        var municipios = new[]
        {
            CreateMockMunicipio(1, "Cidade1"),
            CreateMockMunicipio(2, "Cidade2")
        };
        _mockHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(municipios, SerializationDefaults.Default));

        // Act
        var result = await _client.GetMunicipiosByUFAsync(uf);

        // Assert
        result.Should().HaveCount(2);
        _mockHandler.LastRequestUri.Should().Contain($"estados/{uf.ToUpperInvariant()}/municipios");
    }

    [Fact]
    public async Task GetMunicipiosByUFAsync_WhenApiReturnsError_ShouldReturnEmptyList()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.InternalServerError, "");

        // Act
        var result = await _client.GetMunicipiosByUFAsync("MG");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateCityInStateAsync_WhenCityMatchesState_ShouldReturnTrue()
    {
        // Arrange
        var municipio = CreateMockMunicipio(3104502, "Muriaé", "MG");
        _mockHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { municipio }, SerializationDefaults.Default));

        // Act
        var result = await _client.ValidateCityInStateAsync("Muriaé", "MG");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCityInStateAsync_WhenCityDoesNotMatchState_ShouldReturnFalse()
    {
        // Arrange
        var municipio = CreateMockMunicipio(3104502, "Muriaé", "MG");
        _mockHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(new[] { municipio }, SerializationDefaults.Default));

        // Act
        var result = await _client.ValidateCityInStateAsync("Muriaé", "RJ");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCityInStateAsync_WhenCityNotFound_ShouldReturnFalse()
    {
        // Arrange
        _mockHandler.SetResponse(HttpStatusCode.OK, "[]");

        // Act
        var result = await _client.ValidateCityInStateAsync("CidadeInexistente", "MG");

        // Assert
        result.Should().BeFalse();
    }

    // Helper Methods

    private static Municipio CreateMockMunicipio(int id, string nome, string? ufSigla = "MG")
    {
        return new Municipio
        {
            Id = id,
            Nome = nome,
            Microrregiao = new Microrregiao
            {
                Id = 31024,
                Nome = "Muriaé",
                Mesorregiao = new Mesorregiao
                {
                    Id = 3106,
                    Nome = "Zona da Mata",
                    UF = new UF
                    {
                        Id = 31,
                        Nome = ufSigla == "MG" ? "Minas Gerais" : ufSigla == "RJ" ? "Rio de Janeiro" : "Espírito Santo",
                        Sigla = ufSigla ?? "MG",
                        Regiao = new Regiao
                        {
                            Id = 3,
                            Nome = "Sudeste",
                            Sigla = "SE"
                        }
                    }
                }
            }
        };
    }

    // Mock HttpMessageHandler for testing

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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
