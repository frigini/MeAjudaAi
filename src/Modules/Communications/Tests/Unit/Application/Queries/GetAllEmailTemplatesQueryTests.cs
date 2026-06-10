using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Shared.Caching;

namespace MeAjudaAi.Modules.Communications.Tests.Unit.Application.Queries;

public class GetAllEmailTemplatesQueryTests
{
    [Fact]
    public void GetCacheKey_ShouldReturnCorrectKey()
    {
        // Arrange
        var query = new GetAllEmailTemplatesQuery(Guid.NewGuid());

        // Act
        var cacheKey = query.GetCacheKey();

        // Assert
        cacheKey.Should().Be("email-templates:all");
    }

    [Fact]
    public void GetCacheExpiration_ShouldReturn1Hour()
    {
        // Arrange
        var query = new GetAllEmailTemplatesQuery(Guid.NewGuid());

        // Act
        var expiration = query.GetCacheExpiration();

        // Assert
        expiration.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void GetCacheTags_ShouldReturnCorrectTags()
    {
        // Arrange
        var query = new GetAllEmailTemplatesQuery(Guid.NewGuid());

        // Act
        var tags = query.GetCacheTags();

        // Assert
        tags.Should().NotBeNull();
        tags.Should().Contain(CacheTags.Communications);
        tags.Should().Contain(CacheTags.EmailTemplates);
        tags!.Count.Should().Be(2);
    }
}
