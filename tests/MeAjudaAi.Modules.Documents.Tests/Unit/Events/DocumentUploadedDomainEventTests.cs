using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Events;

public sealed class DocumentUploadedDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var fileUrl = "https://storage.example.com/docs/doc123.pdf";

        // Act
        var @event = new DocumentUploadedDomainEvent(
            aggregateId,
            version,
            providerId,
            documentType,
            fileUrl);

        // Assert
        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.ProviderId.Should().Be(providerId);
        @event.DocumentType.Should().Be(documentType);
        @event.FileUrl.Should().Be(fileUrl);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(EDocumentType.IdentityDocument)]
    [InlineData(EDocumentType.ProofOfResidence)]
    [InlineData(EDocumentType.CriminalRecord)]
    [InlineData(EDocumentType.Other)]
    public void Constructor_WithDifferentDocumentTypes_ShouldStoreCorrectType(EDocumentType documentType)
    {
        // Arrange & Act
        var @event = new DocumentUploadedDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            documentType,
            "https://example.com/doc.pdf");

        // Assert
        @event.DocumentType.Should().Be(documentType);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 1;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var fileUrl = "https://storage.example.com/docs/doc123.pdf";

        var event1 = new DocumentUploadedDomainEvent(aggregateId, version, providerId, documentType, fileUrl);
        var event2 = new DocumentUploadedDomainEvent(aggregateId, version, providerId, documentType, fileUrl);

        // Act & Assert
        event1.Should().Be(event2);
        event1.Equals(event2).Should().BeTrue();
        (event1 == event2).Should().BeTrue();
    }
}
