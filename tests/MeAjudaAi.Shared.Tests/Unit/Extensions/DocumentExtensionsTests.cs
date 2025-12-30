using FluentAssertions;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Shared.Tests.Unit.Extensions;

public class DocumentExtensionsTests
{
    [Theory]
    [InlineData("111.444.777-35")] // CPF válido com formatação
    [InlineData("11144477735")]     // CPF válido sem formatação
    [InlineData("529.982.247-25")] // Outro CPF válido
    public void IsValidCpf_WithValidCpf_ShouldReturnTrue(string cpf)
    {
        // Act
        var result = cpf.IsValidCpf();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("000.000.000-00")] // Todos zeros
    [InlineData("111.111.111-11")] // Todos os dígitos iguais
    [InlineData("123.456.789-00")] // Dígitos verificadores inválidos
    [InlineData("12345678900")]     // Dígitos verificadores inválidos
    [InlineData("123")]             // Muito curto
    [InlineData("")]                // Vazio
    [InlineData(null)]              // Nulo
    [InlineData("   ")]             // Espaços em branco
    [InlineData("abc.def.ghi-jk")] // Não numérico
    public void IsValidCpf_WithInvalidCpf_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsValidCpf();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("11.222.333/0001-81")] // CNPJ válido com formatação
    [InlineData("11222333000181")]      // CNPJ válido sem formatação
    [InlineData("34.028.316/0001-03")] // Outro CNPJ válido
    public void IsValidCnpj_WithValidCnpj_ShouldReturnTrue(string cnpj)
    {
        // Act
        var result = cnpj.IsValidCnpj();

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("00.000.000/0000-00")] // Todos zeros
    [InlineData("11.111.111/1111-11")] // Todos os dígitos iguais
    [InlineData("12.345.678/0001-00")] // Dígitos verificadores inválidos
    [InlineData("123456780001")]        // Tamanho inválido
    [InlineData("123")]                 // Muito curto
    [InlineData("")]                    // Vazio
    [InlineData(null)]                  // Nulo
    [InlineData("   ")]                 // Espaços em branco
    [InlineData("ab.cde.fgh/ijkl-mn")] // Não numérico
    public void IsValidCnpj_WithInvalidCnpj_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsValidCnpj();

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
        cpf.IsValidCpf().Should().BeTrue();
    }

    [Fact]
    public void GenerateValidCnpj_ShouldReturnValidCnpj()
    {
        // Act
        var cnpj = DocumentExtensions.GenerateValidCnpj();

        // Assert
        cnpj.Should().NotBeNullOrEmpty();
        cnpj.Length.Should().Be(14);
        cnpj.IsValidCnpj().Should().BeTrue();
    }

    [Fact]
    public void GenerateValidCpf_ShouldReturnDifferentValues()
    {
        // Act
        var cpf1 = DocumentExtensions.GenerateValidCpf();
        var cpf2 = DocumentExtensions.GenerateValidCpf();

        // Assert
        cpf1.Should().NotBe(cpf2);
    }

    [Fact]
    public void GenerateValidCnpj_ShouldReturnDifferentValues()
    {
        // Act
        var cnpj1 = DocumentExtensions.GenerateValidCnpj();
        var cnpj2 = DocumentExtensions.GenerateValidCnpj();

        // Assert
        cnpj1.Should().NotBe(cnpj2);
    }

    [Fact]
    public void IsValidCpf_WithFormatting_ShouldIgnoreSpecialCharacters()
    {
        // Arrange
        var cpfWithDots = "111.444.777-35";
        var cpfWithoutDots = "11144477735";

        // Act & Assert
        cpfWithDots.IsValidCpf().Should().Be(cpfWithoutDots.IsValidCpf());
    }

    [Fact]
    public void IsValidCnpj_WithFormatting_ShouldIgnoreSpecialCharacters()
    {
        // Arrange
        var cnpjWithDots = "11.222.333/0001-81";
        var cnpjWithoutDots = "11222333000181";

        // Act & Assert
        cnpjWithDots.IsValidCnpj().Should().Be(cnpjWithoutDots.IsValidCnpj());
    }

    [Theory]
    [InlineData("222.222.222-22")]
    [InlineData("333.333.333-33")]
    [InlineData("444.444.444-44")]
    public void IsValidCpf_WithRepeatedDigits_ShouldReturnFalse(string cpf)
    {
        // Act
        var result = cpf.IsValidCpf();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("22.222.222/2222-22")]
    [InlineData("33.333.333/3333-33")]
    [InlineData("44.444.444/4444-44")]
    public void IsValidCnpj_WithRepeatedDigits_ShouldReturnFalse(string cnpj)
    {
        // Act
        var result = cnpj.IsValidCnpj();

        // Assert
        result.Should().BeFalse();
    }
}
