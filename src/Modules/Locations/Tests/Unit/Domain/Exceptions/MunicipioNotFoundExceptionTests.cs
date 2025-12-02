using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Domain.Exceptions;

public sealed class MunicipioNotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithCityNameOnly_ShouldSetPropertiesAndMessage()
    {
        // Arrange & Act
        var exception = new MunicipioNotFoundException("Muriaé");

        // Assert
        exception.CityName.Should().Be("Muriaé");
        exception.StateSigla.Should().BeNull();
        exception.Message.Should().Be("Município 'Muriaé' não encontrado na API IBGE");
    }

    [Fact]
    public void Constructor_WithCityNameAndState_ShouldSetPropertiesAndMessage()
    {
        // Arrange & Act
        var exception = new MunicipioNotFoundException("Muriaé", "MG");

        // Assert
        exception.CityName.Should().Be("Muriaé");
        exception.StateSigla.Should().Be("MG");
        exception.Message.Should().Be("Município 'Muriaé' não encontrado na API IBGE para o estado MG");
    }

    [Fact]
    public void Constructor_WithNullStateSigla_ShouldSetPropertiesWithoutStateInMessage()
    {
        // Arrange & Act
        var exception = new MunicipioNotFoundException("São Paulo", null);

        // Assert
        exception.CityName.Should().Be("São Paulo");
        exception.StateSigla.Should().BeNull();
        exception.Message.Should().Be("Município 'São Paulo' não encontrado na API IBGE");
    }

    [Fact]
    public void Constructor_WithEmptyStateSigla_ShouldSetPropertiesWithoutStateInMessage()
    {
        // Arrange & Act
        var exception = new MunicipioNotFoundException("Rio de Janeiro", string.Empty);

        // Assert
        exception.CityName.Should().Be("Rio de Janeiro");
        exception.StateSigla.Should().BeEmpty();
        exception.Message.Should().Be("Município 'Rio de Janeiro' não encontrado na API IBGE");
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetPropertiesAndInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("IBGE API timeout");

        // Act
        var exception = new MunicipioNotFoundException("Itaperuna", "RJ", innerException);

        // Assert
        exception.CityName.Should().Be("Itaperuna");
        exception.StateSigla.Should().Be("RJ");
        exception.Message.Should().Be("Município 'Itaperuna' não encontrado na API IBGE para o estado RJ");
        exception.InnerException.Should().Be(innerException);
        exception.InnerException!.Message.Should().Be("IBGE API timeout");
    }

    [Fact]
    public void Constructor_WithInnerExceptionAndNullState_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");

        // Act
        var exception = new MunicipioNotFoundException("Linhares", null, innerException);

        // Assert
        exception.CityName.Should().Be("Linhares");
        exception.StateSigla.Should().BeNull();
        exception.Message.Should().Be("Município 'Linhares' não encontrado na API IBGE");
        exception.InnerException.Should().Be(innerException);
    }

    [Theory]
    [InlineData("Muriaé", "MG", "Município 'Muriaé' não encontrado na API IBGE para o estado MG")]
    [InlineData("Itaperuna", "RJ", "Município 'Itaperuna' não encontrado na API IBGE para o estado RJ")]
    [InlineData("Linhares", "ES", "Município 'Linhares' não encontrado na API IBGE para o estado ES")]
    [InlineData("São Paulo", "SP", "Município 'São Paulo' não encontrado na API IBGE para o estado SP")]
    public void Constructor_WithDifferentCitiesAndStates_ShouldFormatMessageCorrectly(
        string cityName,
        string stateSigla,
        string expectedMessage)
    {
        // Arrange & Act
        var exception = new MunicipioNotFoundException(cityName, stateSigla);

        // Assert
        exception.CityName.Should().Be(cityName);
        exception.StateSigla.Should().Be(stateSigla);
        exception.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void Exception_ShouldBeOfTypeException()
    {
        // Arrange & Act
        var exception = new MunicipioNotFoundException("Test City");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}
