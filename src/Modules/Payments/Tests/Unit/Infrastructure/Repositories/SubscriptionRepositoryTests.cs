using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Repositories;

public class SubscriptionRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PaymentsDbContext _context;
    private readonly SubscriptionRepository _subscriptionRepository;

    public SubscriptionRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new PaymentsDbContext(options);
        _context.Database.EnsureCreated();
        
        _subscriptionRepository = new SubscriptionRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSubscription_WhenExists()
    {
        // Arrange
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionRepository.GetByIdAsync(sub.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(sub.Id);
    }

    [Fact]
    public async Task GetActiveByProviderIdAsync_ShouldReturnActiveSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var sub = new Subscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate("sub_123", "cus_123");
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionRepository.GetActiveByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(ESubscriptionStatus.Active);
    }

    [Fact]
    public async Task GetLatestByProviderIdAsync_ShouldReturnMostRecentSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var sub1 = new Subscription(providerId, "plan1", Money.FromDecimal(10));
        _context.Subscriptions.Add(sub1);
        await _context.SaveChangesAsync();

        // Usa o construtor internal com Status e CreatedAt
        var sub2 = new Subscription(Guid.NewGuid(), providerId, "plan2", Money.FromDecimal(20), ESubscriptionStatus.Pending, DateTime.UtcNow.AddSeconds(10));
        _context.Subscriptions.Add(sub2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionRepository.GetLatestByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.PlanId.Should().Be("plan2");
    }

    [Fact]
    public async Task GetByExternalIdAsync_ShouldReturnSubscription_WhenExists()
    {
        // Arrange
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        sub.Activate("external_123", "cus_123");
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionRepository.GetByExternalIdAsync("external_123");

        // Assert
        result.Should().NotBeNull();
        result!.ExternalSubscriptionId.Should().Be("external_123");
    }

    [Fact]
    public async Task GetByExternalIdAsync_ShouldThrow_WhenExternalIdIsEmpty()
    {
        // Act
        var act = () => _subscriptionRepository.GetByExternalIdAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        // Act
        sub.Activate("new_id", "cus_id", DateTime.UtcNow.AddMonths(1));
        await _subscriptionRepository.UpdateAsync(sub);

        // Detach to ensure we are reading from the DB
        _context.Entry(sub).State = EntityState.Detached;
        var updated = await _context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sub.Id);

        // Assert
        updated.Should().NotBeNull();
        updated!.ExternalSubscriptionId.Should().Be("new_id");
        updated.Status.Should().Be(ESubscriptionStatus.Active);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
