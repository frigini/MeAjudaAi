using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Ratings;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Ratings;

public class RatingsModuleApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Ratings;

    [Fact]
    public async Task GetProviderRatingAsync_ReturnsCorrectRating()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        using (var scope = Services.CreateScope())
        {
            var ratingsDb = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
            ratingsDb.Reviews.Add(Review.Create(providerId, Guid.NewGuid(), 5, "Great"));
            ratingsDb.Reviews.Add(Review.Create(providerId, Guid.NewGuid(), 3, "Okay"));
            await ratingsDb.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var ratingsApi = scope.ServiceProvider.GetRequiredService<IRatingsModuleApi>();
            
            // Act
            var result = await ratingsApi.GetProviderRatingAsync(providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AverageRating.Should().Be(4.0m);
            result.Value.TotalReviews.Should().Be(2);
        }
    }
}
