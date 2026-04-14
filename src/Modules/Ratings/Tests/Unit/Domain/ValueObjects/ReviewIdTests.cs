using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Unit.Domain.ValueObjects;

public class ReviewIdTests
{
    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrowArgumentException()
    {
        // Act
        Action action = () => new ReviewId(Guid.Empty);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*vazio*");
    }

    [Fact]
    public void ImplicitConversion_FromGuid_ShouldCreateReviewId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        ReviewId reviewId = guid;

        // Assert
        reviewId.Value.Should().Be(guid);
    }

    [Fact]
    public void ImplicitConversion_ToGuid_ShouldReturnGuid()
    {
        // Arrange
        var reviewId = ReviewId.New();

        // Act
        Guid guid = reviewId;

        // Assert
        guid.Should().Be(reviewId.Value);
    }
}
