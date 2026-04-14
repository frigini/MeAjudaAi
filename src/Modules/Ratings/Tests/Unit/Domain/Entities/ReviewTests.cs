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
    public void Create_WithEmptyProviderId_ShouldThrowArgumentException()
    {
        // Act
        Action action = () => Review.Create(Guid.Empty, Guid.NewGuid(), 5, "Comment");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*ProviderId*");
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrowArgumentException()
    {
        // Act
        Action action = () => Review.Create(Guid.NewGuid(), Guid.Empty, 5, "Comment");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*CustomerId*");
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
        review.DomainEvents.OfType<ReviewApprovedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldDoNothing()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Comment");
        review.Approve();
        review.ClearDomainEvents();

        // Act
        review.Approve();

        // Assert
        review.Status.Should().Be(EReviewStatus.Approved);
        review.DomainEvents.Should().BeEmpty();
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
        review.DomainEvents.OfType<ReviewRejectedDomainEvent>().Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Reject_WithInvalidReason_ShouldThrowArgumentException(string invalidReason)
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "Comment");

        // Act
        Action action = () => review.Reject(invalidReason!);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*Motivo*");
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldDoNothing()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 1, "Comment");
        review.Reject("Initial reason");
        review.ClearDomainEvents();

        // Act
        review.Reject("New reason");

        // Assert
        review.Status.Should().Be(EReviewStatus.Rejected);
        review.RejectionReason.Should().Be("Initial reason");
        review.DomainEvents.Should().BeEmpty();
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
    public void MarkAsFlagged_WhenAlreadyFlagged_ShouldDoNothing()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 3, "Comment");
        review.MarkAsFlagged();
        review.ClearDomainEvents();

        // Act
        review.MarkAsFlagged();

        // Assert
        review.Status.Should().Be(EReviewStatus.Flagged);
        review.DomainEvents.Should().BeEmpty();
    }
}
