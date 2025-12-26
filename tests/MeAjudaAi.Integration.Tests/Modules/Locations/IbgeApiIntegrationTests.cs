using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Locations.Infrastructure.ExternalApis.Clients.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Locations;

/// <summary>
/// Integration tests para IBGE Localidades API usando WireMock.
/// Estes testes NÃO fazem chamadas reais à API IBGE - usam stubs configurados no WireMockFixture.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "Locations")]
public sealed class IbgeApiIntegrationTests : BaseApiTest
{
    private IIbgeClient IbgeClient => Services.GetRequiredService<IIbgeClient>();

    #region GetMunicipioByNameAsync Tests

    [Fact]
    public async Task GetMunicipioByNameAsync_Muriae_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Muriaé";

        // Act
        var result = await IbgeClient.GetMunicipioByNameAsync(cityName);

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

    [Fact(Skip = "Intermittent WireMock connection issues in CI - other IBGE tests cover this functionality")]
    public async Task GetMunicipioByNameAsync_Itaperuna_ShouldReturnValidMunicipio()
    {
        // Arrange
        const string cityName = "Itaperuna";

        // Act
        var result = await IbgeClient.GetMunicipioByNameAsync(cityName);

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
    public async Task GetMunicipioByNameAsync_NonExistentCity_ShouldReturnNull()
    {
        // Arrange
        const string cityName = "CityThatDoesNotExist";

        // Act
        var result = await IbgeClient.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().BeNull("Cidade inexistente deve retornar null");
    }

    [Theory]
    [InlineData("São Paulo", "SP")]
    public async Task GetMunicipioByNameAsync_SpecialCharacters_ShouldHandleCorrectly(string cityName, string expectedUF)
    {
        // Act
        var result = await IbgeClient.GetMunicipioByNameAsync(cityName);

        // Assert
        result.Should().NotBeNull($"{cityName} deve existir no WireMock stub com caracteres especiais");
        result!.Nome.Should().Be(cityName);
        result.GetEstadoSigla().Should().Be(expectedUF);
    }

    #endregion

    #region GetMunicipiosByUFAsync Tests

    [Fact]
    public async Task GetMunicipiosByUFAsync_SP_ShouldReturnSaoPauloCities()
    {
        // Arrange
        const string ufSigla = "SP";

        // Act
        var result = await IbgeClient.GetMunicipiosByUFAsync(ufSigla);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty("SP deve ter municípios no WireMock stub");
        result.Should().Contain(m => m.Nome == "São Paulo");

        // Verificar que todos têm UF = SP
        result.Should().OnlyContain(m => m.GetEstadoSigla() == "SP");
    }

    [Fact]
    public async Task GetMunicipiosByUFAsync_InvalidUF_ShouldReturnEmptyList()
    {
        // Arrange
        const string invalidUF = "XX";

        // Act
        var result = await IbgeClient.GetMunicipiosByUFAsync(invalidUF);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty("UF inválido deve retornar lista vazia");
    }

    #endregion

    #region ValidateCityInStateAsync Tests

    [Fact]
    public async Task ValidateCityInStateAsync_ValidCityAndState_ShouldReturnTrue()
    {
        // Arrange
        const string cityName = "Muriaé";
        const string state = "MG";

        // Act
        var result = await IbgeClient.ValidateCityInStateAsync(cityName, state);

        // Assert
        result.Should().BeTrue("Muriaé está em MG");
    }

    [Fact]
    public async Task ValidateCityInStateAsync_ValidCityWrongState_ShouldReturnFalse()
    {
        // Arrange
        const string cityName = "Muriaé";
        const string state = "RJ";

        // Act
        var result = await IbgeClient.ValidateCityInStateAsync(cityName, state);

        // Assert
        result.Should().BeFalse("Muriaé não está em RJ");
    }

    [Fact]
    public async Task ValidateCityInStateAsync_InvalidCity_ShouldReturnFalse()
    {
        // Arrange
        const string cityName = "CityThatDoesNotExist";
        const string state = "MG";

        // Act
        var result = await IbgeClient.ValidateCityInStateAsync(cityName, state);

        // Assert
        result.Should().BeFalse("Cidade inexistente deve retornar false");
    }

    #endregion
}
