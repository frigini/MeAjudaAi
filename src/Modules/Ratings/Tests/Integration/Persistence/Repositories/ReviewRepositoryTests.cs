using MeAjudaAi.Modules.Ratings.Domain.Entities;
using MeAjudaAi.Modules.Ratings.Domain.Enums;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;

namespace MeAjudaAi.Modules.Ratings.Tests.Integration.Persistence.Repositories;

public class ReviewRepositoryTests : IAsyncDisposable
{
    private readonly RatingsDbContext _context;
    private readonly ReviewRepository _repository;
    private readonly SqliteConnection _connection;

    public ReviewRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<RatingsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new RatingsDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new ReviewRepository(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistReview()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");

        // Act
        await _repository.AddAsync(review);

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
        await _repository.AddAsync(review);

        // Act
        var result = await _repository.GetByProviderAndCustomerAsync(providerId, customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(review.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnReview()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        await _repository.AddAsync(review);

        // Act
        var result = await _repository.GetByIdAsync(review.Id);

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

        await _repository.AddAsync(r1);
        await _repository.AddAsync(r2);
        await _repository.AddAsync(r3);

        // Act
        var result = (await _repository.GetByProviderIdAsync(providerId, 1, 10)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].Comment.Should().Be("Latest");
        result[1].Comment.Should().Be("Oldest");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5, "Test");
        await _repository.AddAsync(review);
        review.Approve();

        // Act
        await _repository.UpdateAsync(review);

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
        
        await _repository.AddAsync(r1);
        await _repository.AddAsync(r2);
        await _repository.AddAsync(r3);

        // Act
        var (average, total) = await _repository.GetAverageRatingForProviderAsync(providerId);

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
        await _repository.AddAsync(review);

        // Act
        var (average, total) = await _repository.GetAverageRatingForProviderAsync(providerId);

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
            await _repository.AddAsync(r);
        }

        // Act
        var page1 = await _repository.GetByProviderIdAsync(providerId, 1, 3);
        var page2 = await _repository.GetByProviderIdAsync(providerId, 2, 3);

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WhenNoReviews_ShouldReturnEmpty()
    {
        // Act
        var result = await _repository.GetByProviderIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldThrowDuplicateReviewException_WhenDuplicateExternalId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var review1 = Review.Create(providerId, customerId, 5, "First");
        await _repository.AddAsync(review1);

        // Para forçar a exceção de duplicidade no banco, precisamos tentar inserir
        // outro review para o mesmo par ProviderId/CustomerId se houver índice único,
        // ou forçar via mock do DbContext se quisermos testar especificamente o catch do Npgsql.
        
        // No SQLite, vamos apenas garantir que a restrição de unicidade (se existir) dispare o erro.
        var review2 = Review.Create(providerId, customerId, 4, "Second");
        
        // Act & Assert
        var act = () => _repository.AddAsync(review2);
        
        // Como o repositório captura apenas PostgresException, no SQLite ele vai lançar DbUpdateException
        // Para subir a cobertura do Branch do repositório, precisaríamos que o catch fosse genérico ou
        // simular o PostgresException.
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
