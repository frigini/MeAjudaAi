using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;

namespace MeAjudaAi.Integration.Tests.Modules.Ratings;

[Trait("Category", "Integration")]
[Trait("Module", "Ratings")]
public class RatingsCreateEndpointTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Ratings;

    private async Task<Guid> CreateTestProviderAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        var contactInfo = new ContactInfo("test@test.com", "12345678901");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new ProviderBuilder()
            .WithName("Test Provider")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(businessProfile)
            .Build();

        context.Providers.Add(provider);
        await context.SaveChangesAsync();

        return provider.Id.Value;
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldReturn201()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());

        var request = new { ProviderId = providerId, Rating = 5, Comment = "Excellent service" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        // Verify review was persisted
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
        var review = db.Reviews.SingleOrDefault(r => r.ProviderId == providerId && r.CustomerId == customerId);
        review.Should().NotBeNull();
        review!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task CreateReview_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        AuthConfig.ClearConfiguration();
        var request = new { ProviderId = Guid.NewGuid(), Rating = 5, Comment = "Test" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReview_WithInvalidRating_ShouldReturn400()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        AuthConfig.ConfigureRegularUser(Guid.NewGuid().ToString());

        var request = new { ProviderId = providerId, Rating = 10, Comment = "Invalid rating" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_Duplicate_ShouldReturn400()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());

        var request = new { ProviderId = providerId, Rating = 4, Comment = "First review" };

        // Act - First review
        var response1 = await Client.PostAsJsonAsync("/api/v1/ratings", request);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Duplicate review
        var response2 = await Client.PostAsJsonAsync("/api/v1/ratings", request);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_WithComment_ShouldPersistComment()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());

        var request = new { ProviderId = providerId, Rating = 4, Comment = "Very good professional" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
        var review = db.Reviews.Single(r => r.ProviderId == providerId && r.CustomerId == customerId);
        review.Comment.Should().Be("Very good professional");
    }

    [Fact]
    public async Task CreateReview_WithoutComment_ShouldPersistNullComment()
    {
        // Arrange
        var providerId = await CreateTestProviderAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());

        var request = new { ProviderId = providerId, Rating = 3 };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/ratings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
        var review = db.Reviews.Single(r => r.ProviderId == providerId && r.CustomerId == customerId);
        review.Comment.Should().BeNull();
    }
}
