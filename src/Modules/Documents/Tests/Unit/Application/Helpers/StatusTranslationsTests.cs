using MeAjudaAi.Modules.Documents.Application.Helpers;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Helpers;

[Trait("Category", "Unit")]
public class StatusTranslationsTests
{
    [Theory]
    [InlineData(EDocumentStatus.PendingVerification, "Verificação Pendente")]
    [InlineData(EDocumentStatus.Uploaded, "Enviado")]
    [InlineData(EDocumentStatus.Rejected, "Rejeitado")]
    [InlineData(EDocumentStatus.Verified, "Verificado")]
    public void ToPortuguese_ShouldReturnCorrectTranslation(EDocumentStatus status, string expected)
    {
        // Act
        var result = status.ToPortuguese();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToPortuguese_WithUnknownValue_ShouldReturnToString()
    {
        // Arrange
        var unknownStatus = (EDocumentStatus)99;

        // Act
        var result = unknownStatus.ToPortuguese();

        // Assert
        result.Should().Be("99");
    }
}
