using MeAjudaAi.Modules.Documents.Application.Helpers;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Helpers;

public class StatusTranslationsTests
{
    [Theory]
    [InlineData(EDocumentStatus.PendingVerification, "Verificação Pendente")]
    [InlineData(EDocumentStatus.Uploaded, "Enviado")]
    [InlineData(EDocumentStatus.Rejected, "Rejeitado")]
    [InlineData(EDocumentStatus.Verified, "Verificado")]
    public void ToPortuguese_Should_ReturnCorrectTranslation(EDocumentStatus status, string expected)
    {
        status.ToPortuguese().Should().Be(expected);
    }

    [Fact]
    public void ToPortuguese_WithUnknownStatus_Should_ReturnToString()
    {
        var status = (EDocumentStatus)999;
        status.ToPortuguese().Should().Be("999");
    }
}
