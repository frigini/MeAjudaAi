using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Domain.ValueObjects;

[Trait("Category", "Unit")]
public class DocumentTests
{
    [Theory]
    [InlineData(EDocumentType.CPF, "11144477735")]
    [InlineData(EDocumentType.CNPJ, "12345678000195")]
    [InlineData(EDocumentType.RG, "123456789")]
    [InlineData(EDocumentType.CNH, "12345678901")]
    public void Constructor_WithValidParameters_ShouldCreateDocument(EDocumentType type, string number)
    {
        // Act
        var document = new Document(number, type);

        // Assert
        document.DocumentType.Should().Be(type);
        document.Number.Should().Be(number);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_WithInvalidNumber_ShouldThrowArgumentException(string? invalidNumber)
    {
        // Act & Assert
        var action = () => new Document(invalidNumber!, EDocumentType.CPF);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Número do documento não pode ser vazio*");
    }

    [Fact]
    public void Constructor_WithNumberTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longNumber = new string('1', 51); // Mais de 50 caracteres

        // Act & Assert
        var action = () => new Document(longNumber, EDocumentType.CPF);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var document1 = new Document("11144477735", EDocumentType.CPF);
        var document2 = new Document("11144477735", EDocumentType.CPF);

        // Act & Assert
        document1.Should().Be(document2);
        document1.GetHashCode().Should().Be(document2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var document1 = new Document("11144477735", EDocumentType.CPF);
        var document2 = new Document("11222333000181", EDocumentType.CNPJ);

        // Act & Assert
        document1.Should().NotBe(document2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var document = new Document("11144477735", EDocumentType.CPF);

        // Act
        var result = document.ToString();

        // Assert
        result.Should().Be("CPF: 11144477735");
    }
}
