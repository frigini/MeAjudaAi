using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Events;

public sealed class DocumentRejectedDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var rejectionReason = "Documento ilegível - baixa qualidade da imagem";

        // Act
        var @event = new DocumentRejectedDomainEvent(
            aggregateId,
            version,
            providerId,
            documentType,
            rejectionReason);

        // Assert
        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.ProviderId.Should().Be(providerId);
        @event.DocumentType.Should().Be(documentType);
        @event.RejectionReason.Should().Be(rejectionReason);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("Documento vencido")]
    [InlineData("Informações inconsistentes")]
    [InlineData("Imagem com qualidade insuficiente")]
    public void Constructor_WithDifferentRejectionReasons_ShouldStoreCorrectReason(string rejectionReason)
    {
        // Arrange & Act
        var @event = new DocumentRejectedDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            EDocumentType.ProofOfResidence,
            rejectionReason);

        // Assert
        @event.RejectionReason.Should().Be(rejectionReason);
    }
}
