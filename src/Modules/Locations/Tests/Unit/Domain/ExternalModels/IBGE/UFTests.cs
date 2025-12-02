using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.ExternalModels.IBGE;

public sealed class UFTests
{
    [Fact]
    public void UF_WithCompleteData_ShouldMapAllProperties()
    {
        // Arrange & Act
        var uf = new UF
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
        };

        // Assert
        uf.Id.Should().Be(31);
        uf.Nome.Should().Be("Minas Gerais");
        uf.Sigla.Should().Be("MG");
        uf.Regiao.Should().NotBeNull();
        uf.Regiao!.Id.Should().Be(3);
        uf.Regiao.Nome.Should().Be("Sudeste");
        uf.Regiao.Sigla.Should().Be("SE");
    }

    [Fact]
    public void UF_WithNullRegiao_ShouldAllowNullRegiao()
    {
        // Arrange & Act
        var uf = new UF
        {
            Id = 31,
            Nome = "Minas Gerais",
            Sigla = "MG",
            Regiao = null
        };

        // Assert
        uf.Id.Should().Be(31);
        uf.Nome.Should().Be("Minas Gerais");
        uf.Sigla.Should().Be("MG");
        uf.Regiao.Should().BeNull();
    }

    [Theory]
    [InlineData(31, "Minas Gerais", "MG")]
    [InlineData(33, "Rio de Janeiro", "RJ")]
    [InlineData(32, "Espírito Santo", "ES")]
    [InlineData(35, "São Paulo", "SP")]
    public void UF_WithDifferentStates_ShouldMapCorrectly(int id, string nome, string sigla)
    {
        // Arrange & Act
        var uf = new UF
        {
            Id = id,
            Nome = nome,
            Sigla = sigla,
            Regiao = new Regiao { Id = 3, Nome = "Sudeste", Sigla = "SE" }
        };

        // Assert
        uf.Id.Should().Be(id);
        uf.Nome.Should().Be(nome);
        uf.Sigla.Should().Be(sigla);
    }
}
