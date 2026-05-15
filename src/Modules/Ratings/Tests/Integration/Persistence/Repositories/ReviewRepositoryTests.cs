using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Domain.Repositories;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.Persistence.Repositories;

public class ReviewRepositoryTests : IAsyncDisposable
{
    private readonly RatingsDbContext _context;
    private readonly ReviewRepository _repository;

    public ReviewRepositoryTests()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseSqlite(connection)
            .Options;

        _context = new RatingsDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new ReviewRepository(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistReview()
    {
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");

        await _repository.AddAsync(review);

        var persisted = await _context.Reviews.FindAsync(review.Id);
        persisted.Should().NotBeNull();
        persisted!.Rating.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        await _repository.AddAsync(review);
        review.Approve();

        await _repository.UpdateAsync(review);

        var persisted = await _context.Reviews.FindAsync(review.Id);
        persisted!.Status.Should().Be(EReviewStatus.Approved);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowDuplicateKeyException_WhenDuplicateProviderAndCustomer()
    {
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review1 = Review.Create(providerId, customerId, 5, "First");
        await _repository.AddAsync(review1);

        var review2 = Review.Create(providerId, customerId, 4, "Second");

        var act = () => _repository.AddAsync(review2);

        await act.Should().ThrowAsync<DbUpdateException>();
    }
}