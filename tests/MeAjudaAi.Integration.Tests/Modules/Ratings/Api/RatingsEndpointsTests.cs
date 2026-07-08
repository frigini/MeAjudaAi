using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using System.Net.Http.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Ratings.Api;

[Trait("Category", "Integration")]
[Trait("Module", "Ratings")]
public class RatingsEndpointsTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Ratings;

    private async Task<Guid> SeedReviewAsync(
        Guid providerId, Guid customerId,
        EReviewStatus status = EReviewStatus.Approved,
        int rating = 5, string? comment = "Great service")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
        var review = Review.Create(providerId, customerId, rating, comment);

        if (status == EReviewStatus.Approved) review.Approve();
        else if (status == EReviewStatus.Rejected) review.Reject("Rejected");
        else if (status == EReviewStatus.Flagged) review.MarkAsFlagged();

        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        return review.Id.Value;
    }

    #region GET /api/v1/ratings/{id}

    [Fact]
    public async Task GetReviewById_ExistingApproved_ShouldReturn200()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid());

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ProviderReviewResponse>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(reviewId);
        dto.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetReviewById_NonExisting_ShouldReturn404()
    {
        var fakeId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/v1/ratings/{fakeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReviewById_PendingReview_ShouldReturn404()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid(), EReviewStatus.Pending);

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReviewById_RejectedReview_ShouldReturn404()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid(), EReviewStatus.Rejected);

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/v1/ratings/provider/{providerId}

    [Fact]
    public async Task GetProviderReviews_WithApprovedReviews_ShouldReturn200()
    {
        var providerId = Guid.NewGuid();
        await SeedReviewAsync(providerId, Guid.NewGuid(), EReviewStatus.Approved, 5, "Excellent");
        await SeedReviewAsync(providerId, Guid.NewGuid(), EReviewStatus.Approved, 3, "Okay");

        var response = await Client.GetAsync($"/api/v1/ratings/provider/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProviderReviewResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
    }

    [Fact]
    public async Task GetProviderReviews_NoReviews_ShouldReturn200WithEmpty()
    {
        var providerId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/v1/ratings/provider/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProviderReviewResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
    }

    [Fact]
    public async Task GetProviderReviews_WithPagination_ShouldRespectPageSize()
    {
        var providerId = Guid.NewGuid();
        for (int i = 0; i < 5; i++)
            await SeedReviewAsync(providerId, Guid.NewGuid(), EReviewStatus.Approved, 5, $"Review {i}");

        var response = await Client.GetAsync($"/api/v1/ratings/provider/{providerId}?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProviderReviewResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.PageSize.Should().Be(2);
        result.TotalItems.Should().Be(5);
    }

    [Fact]
    public async Task GetProviderReviews_OnlyApprovedReviews_ShouldExcludeRejected()
    {
        var providerId = Guid.NewGuid();
        await SeedReviewAsync(providerId, Guid.NewGuid(), EReviewStatus.Approved, 5, "Approved");
        await SeedReviewAsync(providerId, Guid.NewGuid(), EReviewStatus.Rejected, 1, "Rejected");

        var response = await Client.GetAsync($"/api/v1/ratings/provider/{providerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProviderReviewResponse>>();
        result!.Items.Should().HaveCount(1);
        result.Items.First().Comment.Should().Be("Approved");
    }

    #endregion

    #region GET /api/v1/ratings/{id}/status (Admin)

    [Fact]
    public async Task GetReviewStatus_Admin_ExistingReview_ShouldReturn200()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid(), EReviewStatus.Approved);
        AuthConfig.ConfigureAdmin();

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await ReadJsonAsync<ReviewStatusResponse>(response.Content);
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(reviewId);
        dto.Status.Should().Be(Contracts.Modules.Ratings.Enums.EReviewStatus.Approved);
    }

    [Fact]
    public async Task GetReviewStatus_Admin_NonExisting_ShouldReturn404()
    {
        AuthConfig.ConfigureAdmin();
        var fakeId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/v1/ratings/{fakeId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReviewStatus_Unauthenticated_ShouldReturn401()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid());
        AuthConfig.ClearConfiguration();

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReviewStatus_NonAdmin_ShouldReturn403()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid());
        AuthConfig.ConfigureRegularUser();

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetReviewStatus_Admin_PendingReview_ShouldReturn200WithPendingStatus()
    {
        var reviewId = await SeedReviewAsync(Guid.NewGuid(), Guid.NewGuid(), EReviewStatus.Pending);
        AuthConfig.ConfigureAdmin();

        var response = await Client.GetAsync($"/api/v1/ratings/{reviewId}/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await ReadJsonAsync<ReviewStatusResponse>(response.Content);
        dto!.Status.Should().Be(Contracts.Modules.Ratings.Enums.EReviewStatus.Pending);
    }

    #endregion
}
