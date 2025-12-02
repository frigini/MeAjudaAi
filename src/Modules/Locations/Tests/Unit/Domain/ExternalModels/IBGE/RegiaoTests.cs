using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.ExternalModels.IBGE;

public sealed class RegiaoTests
{
    [Fact]
    public void Regiao_WithCompleteData_ShouldMapAllProperties()
    {
        // Arrange & Act
        var regiao = new Regiao
        {
            Id = 3,
            Nome = "Sudeste",
            Sigla = "SE"
        };

        // Assert
        regiao.Id.Should().Be(3);
        regiao.Nome.Should().Be("Sudeste");
        regiao.Sigla.Should().Be("SE");
    }

    [Theory]
    [InlineData(1, "Norte", "N")]
    [InlineData(2, "Nordeste", "NE")]
    [InlineData(3, "Sudeste", "SE")]
    [InlineData(4, "Sul", "S")]
    [InlineData(5, "Centro-Oeste", "CO")]
    public void Regiao_WithBrazilianRegions_ShouldMapCorrectly(int id, string nome, string sigla)
    {
        // Arrange & Act
        var regiao = new Regiao
        {
            Id = id,
            Nome = nome,
            Sigla = sigla
        };

        // Assert
        regiao.Id.Should().Be(id);
        regiao.Nome.Should().Be(nome);
        regiao.Sigla.Should().Be(sigla);
    }

    [Fact]
    public void Regiao_WithEmptyStrings_ShouldAllowEmptyValues()
    {
        // Arrange & Act
        var regiao = new Regiao
        {
            Id = 0,
            Nome = string.Empty,
            Sigla = string.Empty
        };

        // Assert
        regiao.Id.Should().Be(0);
        regiao.Nome.Should().BeEmpty();
        regiao.Sigla.Should().BeEmpty();
    }
}
