using FluentAssertions;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Shared.Tests.Unit.Utilities;

[Trait("Category", "Unit")]
public class SlugHelperTests
{
    [Theory]
    [InlineData("Clinica Auto Muriae", "clinica-auto-muriae")]
    [InlineData("João & Maria Serviços", "joao-maria-servicos")]
    [InlineData("   Espaços   Extras   ", "espacos-extras")]
    [InlineData("C# & .NET Developer", "c-net-developer")]
    [InlineData("Acentuação: áéíóú àèìòù âêîôû äëïöü ãõ ñ ç", "acentuacao-aeiou-aeiou-aeiou-aeiou-ao-n-c")]
    [InlineData("Maçã Verde", "maca-verde")]
    [InlineData("UPPERCASE text", "uppercase-text")]
    [InlineData("multiple---hifens", "multiple-hifens")]
    [InlineData("!@#$%^&*()_+", "")]
    [InlineData("Emoji 🚀 test", "emoji-test")]
    [InlineData("Mixing 123 symbols $$$ and text", "mixing-123-symbols-and-text")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Generate_ShouldReturnExpectedSlug(string? input, string expected)
    {
        // Act
        var result = SlugHelper.Generate(input!);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Some complex text with áccents and SYMBOLS $$$")]
    [InlineData("already-a-slug")]
    [InlineData("Text with 123 and spaces")]
    public void Generate_ShouldBeIdempotent(string input)
    {
        // Act
        var firstPass = SlugHelper.Generate(input);
        var secondPass = SlugHelper.Generate(firstPass);

        // Assert
        secondPass.Should().Be(firstPass);
    }

    [Theory]
    [InlineData("João Maria", "123456", "joao-maria-123456")]
    [InlineData("Clinica ABC", " id-789 ", "clinica-abc-id-789")]
    [InlineData(null, "suffix", "suffix")]
    [InlineData("base", null, "base")]
    [InlineData("!@#", "123", "123")]
    public void GenerateWithSuffix_ShouldReturnExpectedSlug(string? baseText, string? suffix, string expected)
    {
        // Act
        var result = SlugHelper.GenerateWithSuffix(baseText!, suffix!);

        // Assert
        result.Should().Be(expected);
    }
}
