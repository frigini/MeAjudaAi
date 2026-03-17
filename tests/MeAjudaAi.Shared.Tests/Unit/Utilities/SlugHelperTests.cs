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
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Generate_ShouldReturnExpectedSlug(string input, string expected)
    {
        // Act
        var result = SlugHelper.Generate(input);

        // Assert
        result.Should().Be(expected);
    }
}
