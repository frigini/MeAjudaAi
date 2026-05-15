using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.Infrastructure.Queries;

public class DbContextReviewQueriesTests : IAsyncDisposable
{
    private readonly RatingsDbContext _context;
    private readonly DbContextReviewQueries _queries;

    public DbContextReviewQueriesTests()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseSqlite(connection)
            .Options;

        _context = new RatingsDbContext(options);
        _context.Database.EnsureCreated();
        _queries = new DbContextReviewQueries(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnReview()
    {
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        review.Approve();
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var result = await _queries.GetByIdAsync(review.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        var result = await _queries.GetByIdAsync(Domain.ValueObjects.ReviewId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByProviderAndCustomerAsync_ShouldReturnCorrectReview()
    {
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review = Review.Create(providerId, customerId, 4, "Test");
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var result = await _queries.GetByProviderAndCustomerAsync(providerId, customerId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithApprovedReviews_ShouldReturnPaginatedAndOrdered()
    {
        var providerId = Guid.NewGuid();
        var r1 = Review.Create(providerId, Guid.NewGuid(), 4, "Oldest");
        r1.Approve();
        _context.Reviews.Add(r1);

        var r2 = Review.Create(providerId, Guid.NewGuid(), 5, "Latest");
        r2.Approve();
        _context.Reviews.Add(r2);

        var r3 = Review.Create(providerId, Guid.NewGuid(), 1, "Rejected");
        r3.Reject("Bad");
        _context.Reviews.Add(r3);
        await _context.SaveChangesAsync();

        var result = (await _queries.GetByProviderIdAsync(providerId, 1, 10)).ToList();

        result.Should().HaveCount(2);
        result[0].Comment.Should().Be("Latest");
        result[1].Comment.Should().Be("Oldest");
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_ShouldCalculateCorrectly()
    {
        var providerId = Guid.NewGuid();

        var r1 = Review.Create(providerId, Guid.NewGuid(), 5, null);
        r1.Approve();
        _context.Reviews.Add(r1);

        var r2 = Review.Create(providerId, Guid.NewGuid(), 4, null);
        r2.Approve();
        _context.Reviews.Add(r2);

        var r3 = Review.Create(providerId, Guid.NewGuid(), 1, null);
        _context.Reviews.Add(r3);

        await _context.SaveChangesAsync();

        var (average, total) = await _queries.GetAverageRatingForProviderAsync(providerId);

        average.Should().Be(4.5m);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_WhenNoApprovedReviews_ShouldReturnZero()
    {
        var providerId = Guid.NewGuid();
        var review = Review.Create(providerId, Guid.NewGuid(), 5, "Pending");
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var (average, total) = await _queries.GetAverageRatingForProviderAsync(providerId);

        average.Should().Be(0);
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetByProviderIdAsync_Pagination_ShouldReturnCorrectSubset()
    {
        var providerId = Guid.NewGuid();
        for (int i = 1; i <= 5; i++)
        {
            var r = Review.Create(providerId, Guid.NewGuid(), 5, $"Review {i}");
            r.Approve();
            _context.Reviews.Add(r);
        }
        await _context.SaveChangesAsync();

        var page1 = await _queries.GetByProviderIdAsync(providerId, 1, 3);
        var page2 = await _queries.GetByProviderIdAsync(providerId, 2, 3);

        page1.Should().HaveCount(3);
        page2.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WhenNoReviews_ShouldReturnEmpty()
    {
        var result = await _queries.GetByProviderIdAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }
}