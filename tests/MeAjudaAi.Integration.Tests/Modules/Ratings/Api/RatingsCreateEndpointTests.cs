using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Bookings.Domain.Entities;
using MeAjudaAi.Modules.Bookings.Domain.ValueObjects;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Ratings.Api;

[Trait("Category", "Integration")]
[Trait("Module", "Ratings")]
public class RatingsCreateEndpointTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Ratings | TestModule.Providers | TestModule.Bookings;

    private async Task SeedCompletedBookingAsync(Guid providerId, Guid customerId)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();

        if (await context.Bookings.AnyAsync(b => b.ProviderId == providerId && b.ClientId == customerId))
            return;

        var timeSlot = TimeSlot.FromDateTime(DateTime.Today.AddHours(9), DateTime.Today.AddHours(10));
        var booking = Booking.Create(providerId, customerId, TestServiceId, DateOnly.FromDateTime(DateTime.Today), timeSlot);
        booking.Confirm();
        booking.Complete();

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldReturn201()
    {
        // Arrange
        var providerId = await CreateTestProviderViaDbAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());
        await SeedCompletedBookingAsync(providerId, customerId);

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
        var providerId = await CreateTestProviderViaDbAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());
        await SeedCompletedBookingAsync(providerId, customerId);

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
        var providerId = await CreateTestProviderViaDbAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());
        await SeedCompletedBookingAsync(providerId, customerId);

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
        var providerId = await CreateTestProviderViaDbAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());
        await SeedCompletedBookingAsync(providerId, customerId);

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
        var providerId = await CreateTestProviderViaDbAsync();
        var customerId = Guid.NewGuid();
        AuthConfig.ConfigureRegularUser(customerId.ToString());
        await SeedCompletedBookingAsync(providerId, customerId);

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
