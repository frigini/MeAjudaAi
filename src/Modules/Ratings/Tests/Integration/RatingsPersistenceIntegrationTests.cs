using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Queries;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration;

public class RatingsPersistenceIntegrationTests : IAsyncDisposable
{
    private readonly RatingsDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly IReviewQueries _queries;
    private readonly SqliteConnection _connection;

    public RatingsPersistenceIntegrationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new RatingsDbContext(options);
        _context.Database.EnsureCreated();
        _uow = _context;
        _queries = new DbContextReviewQueries(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private IRepository<Review, ReviewId> GetRepository() => _uow.GetRepository<Review, ReviewId>();

    [Fact]
    public async Task Add_ShouldPersistReview()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");

        // Act
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        // Assert
        var persisted = await _context.Reviews.FindAsync(review.Id);
        persisted.Should().NotBeNull();
        persisted!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetByProviderAndCustomerAsync_ShouldReturnCorrectReview()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review = Review.Create(providerId, customerId, 4, "Test");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        // Act
        var result = await _queries.GetByProviderAndCustomerAsync(providerId, customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task TryFindAsync_WithValidId_ShouldReturnReview()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        // Act
        var result = await GetRepository().TryFindAsync(review.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithApprovedReviews_ShouldReturnPaginatedAndOrdered()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var r1 = Review.Create(providerId, Guid.NewGuid(), 4, "Oldest");
        r1.Approve();
        
        await Task.Delay(20); // Garantir timestamps diferentes (SQLite pode ser rápido demais)
        
        var r2 = Review.Create(providerId, Guid.NewGuid(), 5, "Latest");
        r2.Approve();
        
        var r3 = Review.Create(providerId, Guid.NewGuid(), 1, "Rejected");
        r3.Reject("Bad");

        GetRepository().Add(r1);
        GetRepository().Add(r2);
        GetRepository().Add(r3);
        await _uow.SaveChangesAsync();

        // Act
        var result = (await _queries.GetByProviderIdAsync(providerId, 1, 10)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Comment.Should().Be("Latest");
        result[1].Comment.Should().Be("Oldest");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();
        
        review.Approve();

        // Act
        await _uow.SaveChangesAsync();

        // Assert
        var persisted = await _context.Reviews.FindAsync(review.Id);
        persisted!.Status.Should().Be(EReviewStatus.Approved);
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_ShouldCalculateCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        
        var r1 = Review.Create(providerId, Guid.NewGuid(), 5, null);
        r1.Approve();
        var r2 = Review.Create(providerId, Guid.NewGuid(), 4, null);
        r2.Approve();
        var r3 = Review.Create(providerId, Guid.NewGuid(), 1, null); // Not approved
        
        GetRepository().Add(r1);
        GetRepository().Add(r2);
        GetRepository().Add(r3);
        await _uow.SaveChangesAsync();

        // Act
        var (average, total) = await _queries.GetAverageRatingForProviderAsync(providerId);

        // Assert
        average.Should().Be(4.5m);
        total.Should().Be(2);
    }

    [Fact]
    public async Task GetAverageRatingForProviderAsync_WhenNoApprovedReviews_ShouldReturnZero()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var review = Review.Create(providerId, Guid.NewGuid(), 5, "Pending");
        GetRepository().Add(review);
        await _uow.SaveChangesAsync();

        // Act
        var (average, total) = await _queries.GetAverageRatingForProviderAsync(providerId);

        // Assert
        average.Should().Be(0);
        total.Should().Be(0);
    }

    [Fact]
    public async Task GetByProviderIdAsync_Pagination_ShouldReturnCorrectSubset()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        for (int i = 1; i <= 5; i++)
        {
            var r = Review.Create(providerId, Guid.NewGuid(), 5, $"Review {i}");
            r.Approve();
            GetRepository().Add(r);
        }
        await _uow.SaveChangesAsync();

        // Act
        var page1 = await _queries.GetByProviderIdAsync(providerId, 1, 3);
        var page2 = await _queries.GetByProviderIdAsync(providerId, 2, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WhenNoReviews_ShouldReturnEmpty()
    {
        // Act
        var result = await _queries.GetByProviderIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowDbUpdateException_WhenDuplicateProviderAndCustomer()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review1 = Review.Create(providerId, customerId, 5, "First");
        GetRepository().Add(review1);
        await _uow.SaveChangesAsync();

        // Tenta inserir outro review para o mesmo par ProviderId/CustomerId, 
        // o que deve disparar uma violação de chave única no banco de dados.
        var review2 = Review.Create(providerId, customerId, 4, "Second");
        GetRepository().Add(review2);
        
        // Act & Assert
        var act = () => _uow.SaveChangesAsync();
        
        // No ambiente de teste (SQLite), a violação de unicidade lança uma DbUpdateException
        // com uma InnerException do tipo Microsoft.Data.Sqlite.SqliteException.
        var assertions = await act.Should().ThrowAsync<DbUpdateException>();
        var sqliteEx = assertions.Which.InnerException.Should().BeOfType<Microsoft.Data.Sqlite.SqliteException>().Subject;
        sqliteEx.SqliteErrorCode.Should().Be(19); // SQLITE_CONSTRAINT
        sqliteEx.Message.Should().Contain("UNIQUE");
    }
}
