using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Base;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.Infrastructure;

public abstract class RatingsIntegrationTestBase : BaseIntegrationTest
{
    protected override TestInfrastructureOptions GetTestOptions()
    {
        return new TestInfrastructureOptions
        {
            Database = new TestDatabaseOptions
            {
                DatabaseName = $"ratings_test_{GetType().Name}",
                Username = "test_user",
                Password = "test_password",
                Schema = "ratings"
            },
            Cache = new TestCacheOptions { Enabled = false },
            ExternalServices = new TestExternalServicesOptions
            {
                UseMessageBusMock = true
            }
        };
    }

    protected override void ConfigureModuleServices(IServiceCollection services, TestInfrastructureOptions options)
    {
        services.AddRatingsTestInfrastructure(options);
    }

    protected async Task<Review> CreateReviewAsync(
        Guid providerId,
        Guid customerId,
        int rating = 5,
        string? comment = "Test review",
        EReviewStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var review = Review.Create(providerId, customerId, rating, comment);

        if (status == EReviewStatus.Approved)
            review.Approve();
        else if (status == EReviewStatus.Rejected)
            review.Reject("Test rejection");
        else if (status == EReviewStatus.Flagged)
            review.MarkAsFlagged();

        using var scope = CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RatingsDbContext>();
        context.Reviews.Add(review);
        await context.SaveChangesAsync(cancellationToken);
        return review;
    }
}
