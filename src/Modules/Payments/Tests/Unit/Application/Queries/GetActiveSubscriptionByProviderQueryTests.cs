using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Shared.Caching;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Queries;

public class GetActiveSubscriptionByProviderQueryTests
{
    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetActiveSubscriptionByProviderQuery(providerId, Guid.NewGuid());

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().Contain(CacheTags.Payments);
        tags.Should().Contain(CacheTags.ProviderTag(providerId));
    }

    [Fact]
    public void GetCacheKey_ShouldReturnUniqueKey()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetActiveSubscriptionByProviderQuery(providerId, Guid.NewGuid());

        // Act
        var key = query.GetCacheKey();

        // Assert
        key.Should().Be($"subscription:active:provider:{providerId}");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var query = new GetActiveSubscriptionByProviderQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromMinutes(30));
    }
}
