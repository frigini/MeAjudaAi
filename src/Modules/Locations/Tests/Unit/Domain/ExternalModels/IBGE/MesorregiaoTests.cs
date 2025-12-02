using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.ExternalModels.IBGE;

public sealed class MesorregiaoTests
{
    [Fact]
    public void Mesorregiao_WithCompleteData_ShouldMapAllProperties()
    {
        // Arrange & Act
        var mesorregiao = new Mesorregiao
        {
            Id = 3107,
            Nome = "Zona da Mata",
            UF = new UF
            {
                Id = 31,
                Nome = "Minas Gerais",
                Sigla = "MG",
                Regiao = new Regiao
                {
                    Id = 3,
                    Nome = "Sudeste",
                    Sigla = "SE"
                }
            }
        };

        // Assert
        mesorregiao.Id.Should().Be(3107);
        mesorregiao.Nome.Should().Be("Zona da Mata");
        mesorregiao.UF.Should().NotBeNull();
        mesorregiao.UF!.Id.Should().Be(31);
        mesorregiao.UF.Sigla.Should().Be("MG");
    }

    [Fact]
    public void Mesorregiao_WithNullUF_ShouldAllowNullUF()
    {
        // Arrange & Act
        var mesorregiao = new Mesorregiao
        {
            Id = 3107,
            Nome = "Zona da Mata",
            UF = null
        };

        // Assert
        mesorregiao.Id.Should().Be(3107);
        mesorregiao.Nome.Should().Be("Zona da Mata");
        mesorregiao.UF.Should().BeNull();
    }

    [Theory]
    [InlineData(3107, "Zona da Mata", "MG")]
    [InlineData(3301, "Noroeste Fluminense", "RJ")]
    [InlineData(3201, "Noroeste Espírito-santense", "ES")]
    public void Mesorregiao_WithDifferentRegions_ShouldMapCorrectly(int id, string nome, string ufSigla)
    {
        // Arrange & Act
        var mesorregiao = new Mesorregiao
        {
            Id = id,
            Nome = nome,
            UF = new UF
            {
                Id = 1,
                Nome = "Test State",
                Sigla = ufSigla,
                Regiao = new Regiao { Id = 3, Nome = "Sudeste", Sigla = "SE" }
            }
        };

        // Assert
        mesorregiao.Id.Should().Be(id);
        mesorregiao.Nome.Should().Be(nome);
        mesorregiao.UF.Should().NotBeNull();
        mesorregiao.UF!.Sigla.Should().Be(ufSigla);
    }
}
