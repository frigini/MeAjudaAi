using FluentAssertions;
using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration;

public class RatingsPersistenceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private IUnitOfWork _uow = null!;
    private IReviewQueries _queries = null!;
    private RatingsDbContext _context = null!;

    public RatingsPersistenceIntegrationTests()
    {
        var options = new TestDatabaseOptions
        {
            DatabaseName = "ratings_test",
            Username = "test_user",
            Password = "test_password",
            Schema = "ratings"
        };

        _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
            .WithDatabase(options.DatabaseName)
            .WithUsername(options.Username)
            .WithPassword(options.Password)
            .WithCleanUp(true)
            .Build();
    }

    private async Task InitializeInternalAsync()
    {
        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        _context = new RatingsDbContext(options);
        await _context.Database.MigrateAsync();

        _uow = _context;
        _queries = new DbContextReviewQueries(_context);
    }

    private IRepository<Review, ReviewId> GetRepository() => _uow.GetRepository<Review, ReviewId>();

    [Fact]
    public async Task Add_ShouldPersistReview()
    {
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");

        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        var persisted = await _context.Reviews.FindAsync(review.Id);
        persisted.Should().NotBeNull();
        persisted!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetByProviderAndCustomerAsync_ShouldReturnCorrectReview()
    {
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review = Review.Create(providerId, customerId, 4, "Test");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        var result = await _queries.GetByProviderAndCustomerAsync(providerId, customerId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task TryFindAsync_WithValidId_ShouldReturnReview()
    {
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        var result = await GetRepository().TryFindAsync(review.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithApprovedReviews_ShouldReturnPaginatedAndOrdered()
    {
        var providerId = Guid.NewGuid();
        var r1 = Review.Create(providerId, Guid.NewGuid(), 4, "Oldest");
        r1.Approve();
        
        await Task.Delay(20);
        
        var r2 = Review.Create(providerId, Guid.NewGuid(), 5, "Latest");
        r2.Approve();
        
        var r3 = Review.Create(providerId, Guid.NewGuid(), 1, "Rejected");
        r3.Reject("Bad");

        GetRepository().Add(r1);
        GetRepository().Add(r2);
        GetRepository().Add(r3);
        await _uow.SaveChangesAsync();

        var result = (await _queries.GetByProviderIdAsync(providerId, 1, 10)).ToList();

        result.Should().HaveCount(2);
        result[0].Comment.Should().Be("Latest");
        result[1].Comment.Should().Be("Oldest");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();
        
        review.Approve();

        await _uow.SaveChangesAsync();

        var persisted = await _context.Reviews.FindAsync(review.Id);
        persisted!.Status.Should().Be(EReviewStatus.Approved);
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_ShouldCalculateCorrectly()
    {
        var providerId = Guid.NewGuid();
        
        var r1 = Review.Create(providerId, Guid.NewGuid(), 5, null);
        r1.Approve();
        var r2 = Review.Create(providerId, Guid.NewGuid(), 4, null);
        r2.Approve();
        var r3 = Review.Create(providerId, Guid.NewGuid(), 1, null);
        
        GetRepository().Add(r1);
        GetRepository().Add(r2);
        GetRepository().Add(r3);
        await _uow.SaveChangesAsync();

        var (average, total) = await _queries.GetAverageRatingForProviderAsync(providerId);

        average.Should().Be(4.5m);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_WhenNoApprovedReviews_ShouldReturnZero()
    {
        var providerId = Guid.NewGuid();
        var review = Review.Create(providerId, Guid.NewGuid(), 5, "Pending");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

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
            GetRepository().Add(r);
        }
        await _uow.SaveChangesAsync();

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

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowDbUpdateException_WhenDuplicateProviderAndCustomer()
    {
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review1 = Review.Create(providerId, customerId, 5, "First");
        GetRepository().Add(review1);
        await _uow.SaveChangesAsync();

        var review2 = Review.Create(providerId, customerId, 4, "Second");
        GetRepository().Add(review2);
        
        var act = () => _uow.SaveChangesAsync();
        
        var assertions = await act.Should().ThrowAsync<DbUpdateException>();
        var postgresException = assertions.Which.InnerException as Npgsql.PostgresException;
        postgresException.Should().NotBeNull();
        postgresException!.SqlState.Should().Be("23505"); // UniqueViolation
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await InitializeInternalAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}