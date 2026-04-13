using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Events;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Domain.Entities;

public class ReviewTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateReviewAndAddDomainEvent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var rating = 5;
        var comment = "Excellent service!";

        // Act
        var review = Review.Create(providerId, customerId, rating, comment);

        // Assert
        review.Should().NotBeNull();
        review.ProviderId.Should().Be(providerId);
        review.CustomerId.Should().Be(customerId);
        review.Rating.Should().Be(rating);
        review.Comment.Should().Be(comment);
        review.Status.Should().Be(EReviewStatus.Pending);
        review.DomainEvents.Should().ContainSingle(e => e is ReviewCreatedDomainEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Create_WithInvalidRating_ShouldThrowArgumentOutOfRangeException(int invalidRating)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        // Act
        Action action = () => Review.Create(providerId, customerId, invalidRating, "Comment");

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Approve_WhenPending_ShouldChangeStatusToApprovedAndAddDomainEvent()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Comment");

        // Act
        review.Approve();

        // Assert
        review.Status.Should().Be(EReviewStatus.Approved);
        review.DomainEvents.Should().Contain(e => e is ReviewApprovedDomainEvent);
    }

    [Fact]
    public void Reject_WithReason_ShouldChangeStatusToRejectedAndAddDomainEvent()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "Spam");
        var reason = "Contains offensive language";

        // Act
        review.Reject(reason);

        // Assert
        review.Status.Should().Be(EReviewStatus.Rejected);
        review.RejectionReason.Should().Be(reason);
        review.DomainEvents.Should().ContainSingle(e => e is ReviewRejectedDomainEvent);
        var domainEvent = review.DomainEvents.OfType<ReviewRejectedDomainEvent>().First();
        domainEvent.Reason.Should().Be(reason);
        domainEvent.AggregateId.Should().Be(review.Id.Value);
    }

    [Fact]
    public void MarkAsFlagged_ShouldChangeStatusToFlagged()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 3, "Suspicious");

        // Act
        review.MarkAsFlagged();

        // Assert
        review.Status.Should().Be(EReviewStatus.Flagged);
    }

    [Fact]
    public void Approve_WhenPreviouslyRejected_ShouldClearRejectionReason()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Fixed");
        review.Reject("Bad content");

        // Act
        review.Approve();

        // Assert
        review.Status.Should().Be(EReviewStatus.Approved);
        review.RejectionReason.Should().BeNull();
    }
}
