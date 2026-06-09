using FluentAssertions;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Caching;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Application.Queries;

public class GetBookingsByProviderQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow;
        var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid(), 2, 20, from, to);

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be($"bookings-provider:{providerId}:2:20:{from}:{to}");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn5Minutes()
    {
        // Arrange
        var query = new GetBookingsByProviderQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid());

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().Contain(CacheTags.Bookings);
        tags.Should().Contain(CacheTags.ProviderBookingsTag(providerId));
        tags!.Count.Should().Be(2);
    }
}
