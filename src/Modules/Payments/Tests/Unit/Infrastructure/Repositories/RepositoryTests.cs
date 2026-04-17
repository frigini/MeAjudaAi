using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly PaymentsDbContext _context;
    private readonly SubscriptionRepository _subscriptionRepository;
    private readonly PaymentTransactionRepository _transactionRepository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PaymentsDbContext(options);
        _subscriptionRepository = new SubscriptionRepository(_context);
        _transactionRepository = new PaymentTransactionRepository(_context);
    }

    [Fact]
    public async Task SubscriptionRepository_GetByIdAsync_ShouldReturnSubscription()
    {
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        var result = await _subscriptionRepository.GetByIdAsync(sub.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sub.Id);
    }

    [Fact]
    public async Task SubscriptionRepository_GetActiveByProviderIdAsync_ShouldReturnActive()
    {
        var providerId = Guid.NewGuid();
        var sub = new Subscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate("sub_123", "cus_123");
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        var result = await _subscriptionRepository.GetActiveByProviderIdAsync(providerId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(ESubscriptionStatus.Active);
    }

    [Fact]
    public async Task SubscriptionRepository_GetLatestByProviderIdAsync_ShouldReturnLatest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var sub1 = new Subscription(providerId, "plan1", Money.FromDecimal(10));
        _context.Subscriptions.Add(sub1);
        await _context.SaveChangesAsync();

        // Força uma data posterior para o segundo objeto via reflexão.
        // NOTA: O uso de reflexão aqui é um workaround intencional para o banco de dados em memória (InMemoryDatabase),
        // que não emula ordenação temporal real de forma confiável para testes de lógica "Latest".
        var sub2 = new Subscription(providerId, "plan2", Money.FromDecimal(20));
        typeof(MeAjudaAi.Shared.Domain.BaseEntity).GetProperty("CreatedAt")?.SetValue(sub2, DateTime.UtcNow.AddSeconds(1));
        
        _context.Subscriptions.Add(sub2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _subscriptionRepository.GetLatestByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.PlanId.Should().Be("plan2");
    }

    [Fact]
    public async Task SubscriptionRepository_GetByExternalIdAsync_ShouldReturnSubscription()
    {
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        sub.Activate("external_123", "cus_123");
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        var result = await _subscriptionRepository.GetByExternalIdAsync("external_123");

        result.Should().NotBeNull();
        result!.ExternalSubscriptionId.Should().Be("external_123");
    }

    [Fact]
    public async Task SubscriptionRepository_GetByExternalIdAsync_ShouldThrow_WhenEmpty()
    {
        var act = () => _subscriptionRepository.GetByExternalIdAsync("");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SubscriptionRepository_UpdateAsync_ShouldSave()
    {
        var sub = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        _context.Subscriptions.Add(sub);
        await _context.SaveChangesAsync();

        sub.Activate("new_id", "cus_id", DateTime.UtcNow.AddMonths(1));
        await _subscriptionRepository.UpdateAsync(sub);

        // Limpa o rastreamento para garantir leitura do banco
        _context.Entry(sub).State = EntityState.Detached;

        var updated = await _context.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sub.Id);
        updated.Should().NotBeNull();
        updated!.ExternalSubscriptionId.Should().Be("new_id");
    }

    [Fact]
    public async Task PaymentTransactionRepository_GetByExternalIdAsync_ShouldReturnTransaction()
    {
        var subId = Guid.NewGuid();
        var tx = new PaymentTransaction(subId, Money.FromDecimal(10));
        tx.Settle("tx_123");
        _context.PaymentTransactions.Add(tx);
        await _context.SaveChangesAsync();

        var result = await _transactionRepository.GetByExternalIdAsync("tx_123");

        result.Should().NotBeNull();
        result!.ExternalTransactionId.Should().Be("tx_123");
    }

    [Fact]
    public async Task PaymentTransactionRepository_AddAsync_ShouldPersistTransaction()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var tx = new PaymentTransaction(subId, Money.FromDecimal(10));
        tx.Settle("tx_new");

        // Act
        await _transactionRepository.AddAsync(tx);
        
        // Assert
        var saved = await _transactionRepository.GetByExternalIdAsync("tx_new");
        saved.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
