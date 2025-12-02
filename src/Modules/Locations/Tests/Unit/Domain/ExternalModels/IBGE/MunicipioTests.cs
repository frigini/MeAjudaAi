using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ExternalModels.IBGE;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.ExternalModels.IBGE;

public sealed class MunicipioTests
{
    [Fact]
    public void Municipio_WithCompleteHierarchy_ShouldMapAllProperties()
    {
        // Arrange & Act
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = new Microrregiao
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
            }
        };

        // Assert
        municipio.Id.Should().Be(3143906);
        municipio.Nome.Should().Be("Muriaé");
        municipio.Microrregiao.Should().NotBeNull();
        municipio.Microrregiao!.Id.Should().Be(31038);
        municipio.Microrregiao.Nome.Should().Be("Muriaé");
    }

    [Fact]
    public void GetUF_WithCompleteHierarchy_ShouldReturnUF()
    {
        // Arrange
        var expectedUF = new UF
        {
            Id = 31,
            Nome = "Minas Gerais",
            Sigla = "MG",
            Regiao = new Regiao { Id = 3, Nome = "Sudeste", Sigla = "SE" }
        };

        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = new Microrregiao
            {
                Id = 31038,
                Nome = "Muriaé",
                Mesorregiao = new Mesorregiao
                {
                    Id = 3107,
                    Nome = "Zona da Mata",
                    UF = expectedUF
                }
            }
        };

        // Act
        var uf = municipio.GetUF();

        // Assert
        uf.Should().NotBeNull();
        uf!.Id.Should().Be(31);
        uf.Nome.Should().Be("Minas Gerais");
        uf.Sigla.Should().Be("MG");
        uf.Regiao.Should().NotBeNull();
        uf.Regiao!.Sigla.Should().Be("SE");
    }

    [Fact]
    public void GetUF_WithNullMicrorregiao_ShouldReturnNull()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = null
        };

        // Act
        var uf = municipio.GetUF();

        // Assert
        uf.Should().BeNull();
    }

    [Fact]
    public void GetUF_WithNullMesorregiao_ShouldReturnNull()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = new Microrregiao
            {
                Id = 31038,
                Nome = "Muriaé",
                Mesorregiao = null
            }
        };

        // Act
        var uf = municipio.GetUF();

        // Assert
        uf.Should().BeNull();
    }

    [Fact]
    public void GetUF_WithNullUF_ShouldReturnNull()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = new Microrregiao
            {
                Id = 31038,
                Nome = "Muriaé",
                Mesorregiao = new Mesorregiao
                {
                    Id = 3107,
                    Nome = "Zona da Mata",
                    UF = null
                }
            }
        };

        // Act
        var uf = municipio.GetUF();

        // Assert
        uf.Should().BeNull();
    }

    [Fact]
    public void GetEstadoSigla_WithCompleteHierarchy_ShouldReturnSigla()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = new Microrregiao
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
                        Regiao = new Regiao { Id = 3, Nome = "Sudeste", Sigla = "SE" }
                    }
                }
            }
        };

        // Act
        var sigla = municipio.GetEstadoSigla();

        // Assert
        sigla.Should().Be("MG");
    }

    [Fact]
    public void GetEstadoSigla_WithNullUF_ShouldReturnNull()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = null
        };

        // Act
        var sigla = municipio.GetEstadoSigla();

        // Assert
        sigla.Should().BeNull();
    }

    [Fact]
    public void GetNomeCompleto_WithCompleteHierarchy_ShouldReturnFormattedName()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = new Microrregiao
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
                        Regiao = new Regiao { Id = 3, Nome = "Sudeste", Sigla = "SE" }
                    }
                }
            }
        };

        // Act
        var nomeCompleto = municipio.GetNomeCompleto();

        // Assert
        nomeCompleto.Should().Be("Muriaé - MG");
    }

    [Fact]
    public void GetNomeCompleto_WithNullUF_ShouldReturnNameWithQuestionMarks()
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = 3143906,
            Nome = "Muriaé",
            Microrregiao = null
        };

        // Act
        var nomeCompleto = municipio.GetNomeCompleto();

        // Assert
        nomeCompleto.Should().Be("Muriaé - ??");
    }

    [Theory]
    [InlineData(3143906, "Muriaé", "MG", "Muriaé - MG")]
    [InlineData(3302205, "Itaperuna", "RJ", "Itaperuna - RJ")]
    [InlineData(3203205, "Linhares", "ES", "Linhares - ES")]
    public void GetNomeCompleto_WithDifferentCities_ShouldFormatCorrectly(
        int id,
        string nome,
        string sigla,
        string expectedNomeCompleto)
    {
        // Arrange
        var municipio = new Municipio
        {
            Id = id,
            Nome = nome,
            Microrregiao = new Microrregiao
            {
                Id = 1,
                Nome = "Test",
                Mesorregiao = new Mesorregiao
                {
                    Id = 1,
                    Nome = "Test",
                    UF = new UF
                    {
                        Id = 1,
                        Nome = "Test State",
                        Sigla = sigla,
                        Regiao = new Regiao { Id = 1, Nome = "Test Region", Sigla = "TR" }
                    }
                }
            }
        };

        // Act
        var nomeCompleto = municipio.GetNomeCompleto();

        // Assert
        nomeCompleto.Should().Be(expectedNomeCompleto);
    }
}
