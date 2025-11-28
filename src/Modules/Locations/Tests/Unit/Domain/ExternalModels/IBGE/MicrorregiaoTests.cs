using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.ExternalModels.IBGE;

public sealed class MicrorregiaoTests
{
    [Fact]
    public void Microrregiao_WithCompleteData_ShouldMapAllProperties()
    {
        // Arrange & Act
        var microrregiao = new Microrregiao
        {
            Id = 31038,
            Nome = "Muriaé",
            Mesorregiao = new Mesorregiao
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
            }
        };

        // Assert
        microrregiao.Id.Should().Be(31038);
        microrregiao.Nome.Should().Be("Muriaé");
        microrregiao.Mesorregiao.Should().NotBeNull();
        microrregiao.Mesorregiao!.Id.Should().Be(3107);
        microrregiao.Mesorregiao.Nome.Should().Be("Zona da Mata");
        microrregiao.Mesorregiao.UF.Should().NotBeNull();
        microrregiao.Mesorregiao.UF!.Sigla.Should().Be("MG");
    }

    [Fact]
    public void Microrregiao_WithNullMesorregiao_ShouldAllowNullMesorregiao()
    {
        // Arrange & Act
        var microrregiao = new Microrregiao
        {
            Id = 31038,
            Nome = "Muriaé",
            Mesorregiao = null
        };

        // Assert
        microrregiao.Id.Should().Be(31038);
        microrregiao.Nome.Should().Be("Muriaé");
        microrregiao.Mesorregiao.Should().BeNull();
    }

    [Theory]
    [InlineData(31038, "Muriaé", "Zona da Mata")]
    [InlineData(33012, "Itaperuna", "Noroeste Fluminense")]
    [InlineData(32008, "Linhares", "Litoral Norte Espírito-santense")]
    public void Microrregiao_WithDifferentMicroregions_ShouldMapCorrectly(
        int id,
        string nome,
        string mesorregiaoNome)
    {
        // Arrange & Act
        var microrregiao = new Microrregiao
        {
            Id = id,
            Nome = nome,
            Mesorregiao = new Mesorregiao
            {
                Id = 1,
                Nome = mesorregiaoNome,
                UF = new UF
                {
                    Id = 1,
                    Nome = "Test State",
                    Sigla = "TS",
                    Regiao = new Regiao { Id = 1, Nome = "Test Region", Sigla = "TR" }
                }
            }
        };

        // Assert
        microrregiao.Id.Should().Be(id);
        microrregiao.Nome.Should().Be(nome);
        microrregiao.Mesorregiao.Should().NotBeNull();
        microrregiao.Mesorregiao!.Nome.Should().Be(mesorregiaoNome);
    }
}
