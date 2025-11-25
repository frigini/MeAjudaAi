using FluentAssertions;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Integration tests para IBGE Localidades API.
/// Estes testes fazem chamadas REAIS à API IBGE e são skipped por padrão.
/// Para executar: dotnet test --filter "Category=Integration"
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "ExternalApi")]
public sealed class IbgeApiIntegrationTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IbgeClient _client;

    public IbgeApiIntegrationTests()
    {
        var baseUrl = Environment.GetEnvironmentVariable("IBGE_API_BASE_URL")
            ?? "https://servicodados.ibge.gov.br/api/v1/localidades/";

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
        _client = new IbgeClient(_httpClient, NullLogger<IbgeClient>.Instance);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipioByNameAsync_Muriae_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Muriaé";

        // Act
        var result = await _client.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull("Muriaé deve existir na API IBGE");
        result!.Nome.Should().Be("Muriaé");
        result.Id.Should().Be(3129707, "Código IBGE de Muriaé-MG");

        // Validar hierarquia geográfica completa
        var ufSigla = result.GetEstadoSigla();
        ufSigla.Should().Be("MG");

        result.Microrregiao.Should().NotBeNull();
        result.Microrregiao!.Mesorregiao.Should().NotBeNull();
        result.Microrregiao!.Mesorregiao!.UF.Should().NotBeNull();
        result.Microrregiao!.Mesorregiao!.UF!.Nome.Should().Be("Minas Gerais");
        result.Microrregiao!.Mesorregiao!.UF!.Regiao.Should().NotBeNull();
        result.Microrregiao!.Mesorregiao!.UF!.Regiao!.Nome.Should().Be("Sudeste");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipioByNameAsync_Itaperuna_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Itaperuna";

        // Act
        var result = await _client.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull("Itaperuna deve existir na API IBGE");
        result!.Nome.Should().Be("Itaperuna");
        result.Id.Should().Be(3302270, "Código IBGE de Itaperuna-RJ");

        var ufSigla = result.GetEstadoSigla();
        ufSigla.Should().Be("RJ");

        result.Microrregiao!.Mesorregiao!.UF!.Nome.Should().Be("Rio de Janeiro");
        result.Microrregiao!.Mesorregiao!.UF!.Regiao!.Nome.Should().Be("Sudeste");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipioByNameAsync_Linhares_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Linhares";

        // Act
        var result = await _client.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull("Linhares deve existir na API IBGE");
        result!.Nome.Should().Be("Linhares");
        result.Id.Should().Be(3203205, "Código IBGE de Linhares-ES");

        var ufSigla = result.GetEstadoSigla();
        ufSigla.Should().Be("ES");

        result.Microrregiao!.Mesorregiao!.UF!.Nome.Should().Be("Espírito Santo");
        result.Microrregiao!.Mesorregiao!.UF!.Regiao!.Nome.Should().Be("Sudeste");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipioByNameAsync_NonExistentCity_ShouldReturnNull()
    {
        // Arrange
        const string cityName = "CidadeInexistenteXYZ123";

        // Act
        var result = await _client.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().BeNull("Cidade inexistente não deve ser encontrada");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipiosByUFAsync_MG_ShouldReturnMinasGeraisCities()
    {
        // Arrange
        const string ufSigla = "MG";

        // Act
        var result = await _client.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("Minas Gerais deve ter municípios");
        result.Count.Should().BeGreaterThan(800, "MG tem 853 municípios");

        // Verificar que Muriaé está na lista
        result.Should().Contain(m => m.Nome == "Muriaé" && m.Id == 3129707);

        // Verificar que todos têm UF = MG
        result.Should().OnlyContain(m => m.GetEstadoSigla() == "MG");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipiosByUFAsync_RJ_ShouldReturnRioDeJaneiroCities()
    {
        // Arrange
        const string ufSigla = "RJ";

        // Act
        var result = await _client.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("Rio de Janeiro deve ter municípios");
        result.Count.Should().BeGreaterThan(90, "RJ possui mais de 90 municípios");

        // Verificar que Itaperuna está na lista
        result.Should().Contain(m => m.Nome == "Itaperuna" && m.Id == 3302270);

        // Verificar que todos têm UF = RJ
        result.Should().OnlyContain(m => m.GetEstadoSigla() == "RJ");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task GetMunicipiosByUFAsync_ES_ShouldReturnEspiritoSantoCities()
    {
        // Arrange
        const string ufSigla = "ES";

        // Act
        var result = await _client.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("Espírito Santo deve ter municípios");
        result.Count.Should().BeGreaterThan(75, "ES possui mais de 75 municípios");

        // Verificar que Linhares está na lista
        result.Should().Contain(m => m.Nome == "Linhares" && m.Id == 3203205);

        // Verificar que todos têm UF = ES
        result.Should().OnlyContain(m => m.GetEstadoSigla() == "ES");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task ValidateCityInStateAsync_PilotCities_ShouldAllBeValid()
    {
        // Arrange & Act & Assert
        var muriae = await _client.ValidateCityInStateAsync("Muriaé", "MG");
        muriae.Should().BeTrue("Muriaé-MG deve ser válida");

        var itaperuna = await _client.ValidateCityInStateAsync("Itaperuna", "RJ");
        itaperuna.Should().BeTrue("Itaperuna-RJ deve ser válida");

        var linhares = await _client.ValidateCityInStateAsync("Linhares", "ES");
        linhares.Should().BeTrue("Linhares-ES deve ser válida");
    }

    [Fact(Skip = "Real API call - run manually or in integration test suite")]
    public async Task ValidateCityInStateAsync_WrongState_ShouldReturnFalse()
    {
        // Arrange & Act
        var result = await _client.ValidateCityInStateAsync("Muriaé", "RJ");

        // Assert
        result.Should().BeFalse("Muriaé não pertence ao estado RJ");
    }

    [Theory(Skip = "Real API call - run manually or in integration test suite")]
    [InlineData("São Paulo", "SP")]
    [InlineData("Rio de Janeiro", "RJ")]
    [InlineData("Belo Horizonte", "MG")]
    [InlineData("Vitória", "ES")]
    public async Task GetMunicipioByNameAsync_MajorCities_ShouldReturnValid(string cityName, string expectedUF)
    {
        // Act
        var result = await _client.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull($"{cityName} deve existir na API IBGE");
        result!.Nome.Should().Be(cityName);
        result.GetEstadoSigla().Should().Be(expectedUF);
        result.Microrregiao!.Mesorregiao!.UF!.Regiao!.Nome.Should().Be("Sudeste");
    }
}
