using MeAjudaAi.Contracts.Modules.Ratings;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

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
            var r1 = Review.Create(providerId, Guid.NewGuid(), 5, "Great");
            var r2 = Review.Create(providerId, Guid.NewGuid(), 3, "Okay");
            
            r1.Approve();
            r2.Approve();
            
            ratingsDb.Reviews.Add(r1);
            ratingsDb.Reviews.Add(r2);
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

    [Fact]
    public async Task GetProviderRatingAsync_WhenNoReviews_ReturnsZeroRating()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        using (var scope = Services.CreateScope())
        {
            var ratingsApi = scope.ServiceProvider.GetRequiredService<IRatingsModuleApi>();
            
            // Act
            var result = await ratingsApi.GetProviderRatingAsync(providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.AverageRating.Should().Be(0);
            result.Value.TotalReviews.Should().Be(0);
        }
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_WhenReviewExists_ReturnsTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        using (var scope = Services.CreateScope())
        {
            var ratingsDb = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
            var review = Review.Create(providerId, customerId, 5, "Great");
            ratingsDb.Reviews.Add(review);
            await ratingsDb.SaveChangesAsync();
        }

        using (var scope = Services.CreateScope())
        {
            var ratingsApi = scope.ServiceProvider.GetRequiredService<IRatingsModuleApi>();
            
            // Act
            var result = await ratingsApi.HasCustomerReviewedProviderAsync(customerId, providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }
    }

    [Fact]
    public async Task HasCustomerReviewedProviderAsync_WhenReviewDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        using (var scope = Services.CreateScope())
        {
            var ratingsApi = scope.ServiceProvider.GetRequiredService<IRatingsModuleApi>();
            
            // Act
            var result = await ratingsApi.HasCustomerReviewedProviderAsync(customerId, providerId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }
    }
}
