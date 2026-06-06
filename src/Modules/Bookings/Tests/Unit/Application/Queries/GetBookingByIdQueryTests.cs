using FluentAssertions;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Shared.Caching;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Queries;

public class GetBookingByIdQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var query = new GetBookingByIdQuery(bookingId, null, null, false, Guid.NewGuid());

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be($"booking:{bookingId}");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn15Minutes()
    {
        // Arrange
        var query = new GetBookingByIdQuery(Guid.NewGuid(), null, null, false, Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var query = new GetBookingByIdQuery(bookingId, null, null, false, Guid.NewGuid());

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().Contain(CacheTags.Bookings);
        tags.Should().Contain(CacheTags.BookingTag(bookingId));
        tags!.Count.Should().Be(2);
    }
}
