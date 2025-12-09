using FluentAssertions;
using MeAjudaAi.Shared.Extensions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Component", "Extensions")]
public class DocumentExtensionsTests
{
    #region CPF Validation Tests

    [Theory]
    [InlineData("123.456.789-09")] // Valid CPF with formatting
    [InlineData("12345678909")]     // Valid CPF without formatting
    [InlineData("529.982.247-25")]  // Another valid CPF
    [InlineData("52998224725")]
    public void IsValidCpf_WithValidCpf_ShouldReturnTrue(string cpf)
    {
        // Act
        var result = cpf.IsValidCpf();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("123.456.789-00")] // Invalid check digit
    [InlineData("12345678900")]
    [InlineData("111.111.111-11")] // All same digits
    [InlineData("11111111111")]
    [InlineData("000.000.000-00")] // All zeros
    [InlineData("00000000000")]
    [InlineData("999.999.999-99")] // All nines
    [InlineData("99999999999")]
    public void IsValidCpf_WithInvalidCpf_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsValidCpf();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")] // Empty string
    [InlineData("   ")] // Whitespace
    [InlineData("123")] // Too short
    [InlineData("123456789012345")] // Too long
    [InlineData("abc.def.ghi-jk")] // Non-numeric
    public void IsValidCpf_WithInvalidFormat_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsValidCpf();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCpf_WithNull_ShouldReturnFalse()
    {
        // Arrange
        string? cpf = null;

        // Act
        var result = cpf!.IsValidCpf();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateValidCpf_ShouldReturnValidCpf()
    {
        // Act
        var cpf = DocumentExtensions.GenerateValidCpf();

        // Assert
        cpf.Should().NotBeNullOrEmpty();
        cpf.Length.Should().Be(11);
        cpf.Should().MatchRegex(@"^\d{11}$"); // Only digits
        cpf.IsValidCpf().Should().BeTrue();
    }

    [Fact]
    public void GenerateValidCpf_CalledMultipleTimes_ShouldReturnDifferentCpfs()
    {
        // Act
        var cpf1 = DocumentExtensions.GenerateValidCpf();
        var cpf2 = DocumentExtensions.GenerateValidCpf();
        var cpf3 = DocumentExtensions.GenerateValidCpf();

        // Assert
        cpf1.Should().NotBe(cpf2);
        cpf2.Should().NotBe(cpf3);
        cpf1.Should().NotBe(cpf3);

        // All should be valid
        cpf1.IsValidCpf().Should().BeTrue();
        cpf2.IsValidCpf().Should().BeTrue();
        cpf3.IsValidCpf().Should().BeTrue();
    }

    #endregion

    #region CNPJ Validation Tests

    [Theory]
    [InlineData("11.222.333/0001-81")] // Valid CNPJ with formatting
    [InlineData("11222333000181")]     // Valid CNPJ without formatting
    [InlineData("11.444.777/0001-61")] // Another valid CNPJ
    [InlineData("11444777000161")]
    public void IsValidCnpj_WithValidCnpj_ShouldReturnTrue(string cnpj)
    {
        // Act
        var result = cnpj.IsValidCnpj();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("11.222.333/0001-00")] // Invalid check digit
    [InlineData("11222333000100")]
    [InlineData("11.111.111/1111-11")] // All same digits
    [InlineData("11111111111111")]
    [InlineData("00.000.000/0000-00")] // All zeros
    [InlineData("00000000000000")]
    [InlineData("99.999.999/9999-99")] // All nines
    [InlineData("99999999999999")]
    public void IsValidCnpj_WithInvalidCnpj_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsValidCnpj();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")] // Empty string
    [InlineData("   ")] // Whitespace
    [InlineData("123")] // Too short
    [InlineData("123456789012345678")] // Too long
    [InlineData("ab.cde.fgh/ijkl-mn")] // Non-numeric
    public void IsValidCnpj_WithInvalidFormat_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsValidCnpj();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidCnpj_WithNull_ShouldReturnFalse()
    {
        // Arrange
        string? cnpj = null;

        // Act
        var result = cnpj!.IsValidCnpj();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateValidCnpj_ShouldReturnValidCnpj()
    {
        // Act
        var cnpj = DocumentExtensions.GenerateValidCnpj();

        // Assert
        cnpj.Should().NotBeNullOrEmpty();
        cnpj.Length.Should().Be(14);
        cnpj.Should().MatchRegex(@"^\d{14}$"); // Only digits
        cnpj.IsValidCnpj().Should().BeTrue();
    }

    [Fact]
    public void GenerateValidCnpj_CalledMultipleTimes_ShouldReturnDifferentCnpjs()
    {
        // Act
        var cnpj1 = DocumentExtensions.GenerateValidCnpj();
        var cnpj2 = DocumentExtensions.GenerateValidCnpj();
        var cnpj3 = DocumentExtensions.GenerateValidCnpj();

        // Assert
        cnpj1.Should().NotBe(cnpj2);
        cnpj2.Should().NotBe(cnpj3);
        cnpj1.Should().NotBe(cnpj3);

        // All should be valid
        cnpj1.IsValidCnpj().Should().BeTrue();
        cnpj2.IsValidCnpj().Should().BeTrue();
        cnpj3.IsValidCnpj().Should().BeTrue();
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public void IsValidCpf_WithFormattedString_ShouldRemoveNonNumericCharacters()
    {
        // Arrange - Various formatting styles
        var cpfFormats = new[]
        {
            "123.456.789-09",
            "123 456 789 09",
            "123-456-789-09",
            "123/456/789/09"
        };

        // Act & Assert
        foreach (var cpf in cpfFormats)
        {
            cpf.IsValidCpf().Should().BeTrue($"'{cpf}' should be valid after stripping non-numeric characters");
        }
    }

    [Fact]
    public void IsValidCnpj_WithFormattedString_ShouldRemoveNonNumericCharacters()
    {
        // Arrange - Various formatting styles
        var cnpjFormats = new[]
        {
            "11.222.333/0001-81",
            "11 222 333 0001 81",
            "11-222-333-0001-81",
            "11/222/333/0001/81"
        };

        // Act & Assert
        foreach (var cnpj in cnpjFormats)
        {
            cnpj.IsValidCnpj().Should().BeTrue($"'{cnpj}' should be valid after stripping non-numeric characters");
        }
    }

    [Fact]
    public void GenerateValidCpf_AndValidate_ShouldAlwaysBeValid()
    {
        // Act & Assert - Generate and validate 100 CPFs
        for (int i = 0; i < 100; i++)
        {
            var cpf = DocumentExtensions.GenerateValidCpf();
            cpf.IsValidCpf().Should().BeTrue($"Generated CPF should always be valid (iteration {i})");
        }
    }

    [Fact]
    public void GenerateValidCnpj_AndValidate_ShouldAlwaysBeValid()
    {
        // Act & Assert - Generate and validate 100 CNPJs
        for (int i = 0; i < 100; i++)
        {
            var cnpj = DocumentExtensions.GenerateValidCnpj();
            cnpj.IsValidCnpj().Should().BeTrue($"Generated CNPJ should always be valid (iteration {i})");
        }
    }

    [Theory]
    [InlineData("12345678909", true)]  // Valid CPF
    [InlineData("11222333000181", false)] // Valid CNPJ (not CPF)
    public void IsValidCpf_ShouldNotValidateCnpj(string document, bool expectedResult)
    {
        // Act
        var result = document.IsValidCpf();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("11222333000181", true)]  // Valid CNPJ
    [InlineData("12345678909", false)] // Valid CPF (not CNPJ)
    public void IsValidCnpj_ShouldNotValidateCpf(string document, bool expectedResult)
    {
        // Act
        var result = document.IsValidCnpj();

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion
}
