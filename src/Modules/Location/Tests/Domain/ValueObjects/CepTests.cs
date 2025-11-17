using FluentAssertions;
using MeAjudaAi.Modules.Location.Domain.ValueObjects;
using Xunit;

namespace MeAjudaAi.Modules.Location.Tests.Domain.ValueObjects;

public sealed class CepTests
{
    [Fact]
    public void Create_WithValidCep_ShouldReturnCepObject()
    {
        // Arrange
        var validCep = "12345678";

        // Act
        var cep = Cep.Create(validCep);

        // Assert
        cep.Should().NotBeNull();
        cep!.Value.Should().Be("12345678");
    }

    [Theory]
    [InlineData("12345-678")]
    [InlineData("12345.678")]
    [InlineData("  12345678  ")]
    public void Create_WithFormattedCep_ShouldNormalizeAndReturnCepObject(string input)
    {
        // Act
        var cep = Cep.Create(input);

        // Assert
        cep.Should().NotBeNull();
        cep!.Value.Should().Be("12345678");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567")] // 7 dígitos
    [InlineData("123456789")] // 9 dígitos
    [InlineData("abcd5678")] // Contém letras
    public void Create_WithInvalidCep_ShouldReturnNull(string? invalidCep)
    {
        // Act
        var cep = Cep.Create(invalidCep);

        // Assert
        cep.Should().BeNull();
    }

    [Fact]
    public void Formatted_ShouldReturnCepWithHyphen()
    {
        // Arrange
        var cep = Cep.Create("12345678");

        // Act
        var formatted = cep!.Formatted;

        // Assert
        formatted.Should().Be("12345-678");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedCep()
    {
        // Arrange
        var cep = Cep.Create("12345678");

        // Act
        var result = cep!.ToString();

        // Assert
        result.Should().Be("12345-678");
    }

    [Fact]
    public void Equals_WithSameCep_ShouldReturnTrue()
    {
        // Arrange
        var cep1 = Cep.Create("12345678");
        var cep2 = Cep.Create("12345678");

        // Act & Assert
        cep1.Should().Be(cep2);
        (cep1 == cep2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCep_ShouldReturnFalse()
    {
        // Arrange
        var cep1 = Cep.Create("12345678");
        var cep2 = Cep.Create("87654321");

        // Act & Assert
        cep1.Should().NotBe(cep2);
        (cep1 != cep2).Should().BeTrue();
    }
}
