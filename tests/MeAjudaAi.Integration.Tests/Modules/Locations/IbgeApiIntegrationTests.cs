using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Integration tests para IBGE Localidades API usando WireMock.
/// Estes testes NÃO fazem chamadas reais à API IBGE - usam stubs configurados no WireMockFixture.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "Locations")]
public sealed class IbgeApiIntegrationTests : ApiTestBase
{
    private readonly IbgeClient _ibgeClient;

    public IbgeApiIntegrationTests()
    {
        _ibgeClient = ServiceProvider.GetRequiredService<IbgeClient>();
    }

    [Fact]
    public async Task GetMunicipioByNameAsync_Muriae_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Muriaé";

        // Act
        var result = await _ibgeClient.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull("Muriaé deve existir no WireMock stub");
        result!.Nome.Should().Be("Muriaé");
        result.Id.Should().Be(3143906, "Código IBGE de Muriaé-MG no stub");

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

    [Fact]
    public async Task GetMunicipioByNameAsync_Itaperuna_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Itaperuna";

        // Act
        var result = await _ibgeClient.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull("Itaperuna deve existir no WireMock stub");
        result!.Nome.Should().Be("Itaperuna");
        result.Id.Should().Be(3302205, "Código IBGE de Itaperuna-RJ no stub");

        var ufSigla = result.GetEstadoSigla();
        ufSigla.Should().Be("RJ");

        result.Microrregiao!.Mesorregiao!.UF!.Nome.Should().Be("Rio de Janeiro");
        result.Microrregiao!.Mesorregiao!.UF!.Regiao!.Nome.Should().Be("Sudeste");
    }

    [Fact]
    public async Task GetMunicipioByIdAsync_Muriae_ShouldReturnValidMunicipio()
    {
        // Arrange
        const int ibgeCode = 3143906; // Muriaé-MG

        // Act
        var result = await _ibgeClient.GetMunicipioByIdAsync(ibgeCode);

        // Assert
        result.Should().NotBeNull("Muriaé deve existir no WireMock stub por ID");
        result!.Nome.Should().Be("Muriaé");
        result.Id.Should().Be(ibgeCode);
        result.GetEstadoSigla().Should().Be("MG");
    }

    [Fact]
    public async Task GetEstadosAsync_ShouldReturnSudesteStates()
    {
        // Act
        var result = await _ibgeClient.GetEstadosAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("WireMock stub retorna estados do Sudeste");
        result.Should().HaveCountGreaterThanOrEqualTo(3, "Pelo menos MG, RJ, SP");

        // Verificar que estados esperados estão presentes
        result.Should().Contain(e => e.Sigla == "MG" && e.Nome == "Minas Gerais");
        result.Should().Contain(e => e.Sigla == "RJ" && e.Nome == "Rio de Janeiro");
        result.Should().Contain(e => e.Sigla == "SP" && e.Nome == "São Paulo");
    }

    [Fact]
    public async Task GetEstadoByIdAsync_MG_ShouldReturnMinasGerais()
    {
        // Arrange
        const int estadoId = 31; // MG

        // Act
        var result = await _ibgeClient.GetEstadoByIdAsync(estadoId);

        // Assert
        result.Should().NotBeNull("MG deve existir no WireMock stub");
        result!.Id.Should().Be(31);
        result.Sigla.Should().Be("MG");
        result.Nome.Should().Be("Minas Gerais");
        result.Regiao.Should().NotBeNull();
        result.Regiao!.Nome.Should().Be("Sudeste");
    }

    [Fact]
    public async Task GetEstadoByUFAsync_MG_ShouldReturnMinasGerais()
    {
        // Arrange
        const string uf = "MG";

        // Act
        var result = await _ibgeClient.GetEstadoByUFAsync(uf);

        // Assert
        result.Should().NotBeNull("MG deve existir no WireMock stub por UF");
        result!.Sigla.Should().Be("MG");
        result.Nome.Should().Be("Minas Gerais");
    }

    [Fact]
    public async Task GetMunicipiosByUFAsync_SP_ShouldReturnSaoPauloCities()
    {
        // Arrange
        const string ufSigla = "SP";

        // Act
        var result = await _ibgeClient.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("SP deve ter municípios no WireMock stub");
        result.Should().Contain(m => m.Nome == "São Paulo");

        // Verificar que todos têm UF = SP
        result.Should().OnlyContain(m => m.GetEstadoSigla() == "SP");
    }

    [Fact]
    public async Task GetMunicipioByIdAsync_InvalidId_ShouldReturnNull()
    {
        // Arrange
        const int invalidId = 9999999;

        // Act
        var result = await _ibgeClient.GetMunicipioByIdAsync(invalidId);

        // Assert
        result.Should().BeNull("ID inválido deve retornar null conforme WireMock stub");
    }

    [Fact]
    public async Task GetEstadoByIdAsync_InvalidId_ShouldReturnNull()
    {
        // Arrange
        const int invalidId = 999;

        // Act
        var result = await _ibgeClient.GetEstadoByIdAsync(invalidId);

        // Assert
        result.Should().BeNull("ID de estado inválido deve retornar null conforme WireMock stub");
    }

    [Theory]
    [InlineData("São Paulo", "SP")]
    public async Task GetMunicipioByNameAsync_SpecialCharacters_ShouldHandleCorrectly(string cityName, string expectedUF)
    {
        // Act
        var result = await _ibgeClient.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull($"{cityName} deve existir no WireMock stub com caracteres especiais");
        result!.Nome.Should().Be(cityName);
        result.GetEstadoSigla().Should().Be(expectedUF);
    }
}
