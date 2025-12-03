using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.ValueObjects;

public sealed class CepTests
{
    [Theory]
    [InlineData("12345678", "12345678")]
    [InlineData("12345-678", "12345678")]
    [InlineData("01310-100", "01310100")]
    public void Create_WithValidCep_ShouldReturnCepObject(string validCep, string expectedValue)
    {
        // Act
        var cep = Cep.Create(validCep);

        // Assert
        cep.Should().NotBeNull();
        cep!.Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnNull(string? invalidCep)
    {
        // Act
        var cep = Cep.Create(invalidCep);

        // Assert
        cep.Should().BeNull();
    }

    [Theory]
    [InlineData("1234567")]     // 7 dígitos
    [InlineData("123456789")]   // 9 dígitos
    [InlineData("abcdefgh")]    // letras
    [InlineData("1234-5678")]   // formato inválido
    public void Create_WithInvalidFormat_ShouldReturnNull(string invalidCep)
    {
        // Act
        var cep = Cep.Create(invalidCep);

        // Assert
        cep.Should().BeNull();
    }

    [Fact]
    public void Create_WithCepContainingDash_ShouldRemoveDash()
    {
        // Arrange
        var cepWithDash = "12345-678";

        // Act
        var cep = Cep.Create(cepWithDash);

        // Assert
        cep.Should().NotBeNull();
        cep!.Value.Should().Be("12345678");
    }

    [Fact]
    public void Formatted_ShouldReturnCepWithDash()
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
        var cep = Cep.Create("01310100");

        // Act
        var result = cep!.ToString();

        // Assert
        result.Should().Be("01310-100");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var cep1 = Cep.Create("12345678");
        var cep2 = Cep.Create("12345-678");

        // Act & Assert
        cep1.Should().Be(cep2);
        (cep1 == cep2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var cep1 = Cep.Create("12345678");
        var cep2 = Cep.Create("87654321");

        // Act & Assert
        cep1.Should().NotBe(cep2);
        (cep1 != cep2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHashCode()
    {
        // Arrange
        var cep1 = Cep.Create("12345678");
        var cep2 = Cep.Create("12345-678");

        // Act & Assert
        cep1!.GetHashCode().Should().Be(cep2!.GetHashCode());
    }

    [Theory]
    [InlineData("12.345-678", "12345678")]
    [InlineData("12 345-678", "12345678")]
    [InlineData(" 12345678 ", "12345678")]
    public void Create_WithExtraCharacters_ShouldCleanAndValidate(string cepWithExtras, string expectedValue)
    {
        // Act
        var cep = Cep.Create(cepWithExtras);

        // Assert
        cep.Should().NotBeNull();
        cep!.Value.Should().Be(expectedValue);
    }
}
