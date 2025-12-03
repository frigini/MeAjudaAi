using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Events;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Events;

public sealed class DocumentFailedDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var providerId = Guid.NewGuid();
        var documentType = EDocumentType.IdentityDocument;
        var failureReason = "OCR service timeout after 30 seconds";

        // Act
        var @event = new DocumentFailedDomainEvent(
            aggregateId,
            version,
            providerId,
            documentType,
            failureReason);

        // Assert
        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.ProviderId.Should().Be(providerId);
        @event.DocumentType.Should().Be(documentType);
        @event.FailureReason.Should().Be(failureReason);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("Network error - connection refused")]
    [InlineData("Azure Document Intelligence service unavailable")]
    [InlineData("Storage service returned 503")]
    public void Constructor_WithDifferentFailureReasons_ShouldStoreCorrectReason(string failureReason)
    {
        // Arrange & Act
        var @event = new DocumentFailedDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            EDocumentType.CriminalRecord,
            failureReason);

        // Assert
        @event.FailureReason.Should().Be(failureReason);
    }

    [Theory]
    [InlineData(EDocumentType.IdentityDocument)]
    [InlineData(EDocumentType.ProofOfResidence)]
    [InlineData(EDocumentType.CriminalRecord)]
    [InlineData(EDocumentType.Other)]
    public void Constructor_WithDifferentDocumentTypes_ShouldStoreCorrectType(EDocumentType documentType)
    {
        // Arrange & Act
        var @event = new DocumentFailedDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            documentType,
            "Service unavailable");

        // Assert
        @event.DocumentType.Should().Be(documentType);
    }
}
