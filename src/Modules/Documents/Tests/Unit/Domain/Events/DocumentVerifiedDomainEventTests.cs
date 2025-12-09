using FluentAssertions;
using MeAjudaAi.Modules.Documents.Domain.Events;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Events;

public sealed class DocumentVerifiedDomainEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var version = 2;
        var verifiedBy = "system-ocr";
        var verifiedAt = DateTime.UtcNow;

        // Act
        var @event = new DocumentVerifiedDomainEvent(
            aggregateId,
            version,
            verifiedBy,
            verifiedAt);

        // Assert
        @event.AggregateId.Should().Be(aggregateId);
        @event.Version.Should().Be(version);
        @event.VerifiedBy.Should().Be(verifiedBy);
        @event.VerifiedAt.Should().Be(verifiedAt);
        @event.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("admin-user-123")]
    [InlineData("ocr-service")]
    [InlineData("manual-reviewer")]
    public void Constructor_WithDifferentVerifiers_ShouldStoreCorrectVerifier(string verifiedBy)
    {
        // Arrange & Act
        var @event = new DocumentVerifiedDomainEvent(
            Guid.NewGuid(),
            1,
            verifiedBy,
            DateTime.UtcNow);

        // Assert
        @event.VerifiedBy.Should().Be(verifiedBy);
    }
}
