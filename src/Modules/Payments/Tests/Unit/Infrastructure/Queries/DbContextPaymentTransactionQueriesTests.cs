using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Queries;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class DbContextPaymentTransactionQueriesTests : IDisposable
{
    private readonly PaymentsDbContext _dbContext;
    private readonly DbContextPaymentTransactionQueries _queries;

    public DbContextPaymentTransactionQueriesTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase("PaymentTransactionQueriesTest_" + Guid.NewGuid())
            .Options;
        _dbContext = new PaymentsDbContext(options);
        _queries = new DbContextPaymentTransactionQueries(_dbContext);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task GetByExternalIdAsync_WithExistingTransaction_ShouldReturnTransaction()
    {
        var subscriptionId = Guid.NewGuid();
        var tx = new PaymentTransaction(subscriptionId, MeAjudaAi.Shared.Domain.ValueObjects.Money.FromDecimal(100m, "BRL"));
        tx.Settle("ext-123");
        _dbContext.PaymentTransactions.Add(tx);
        await _dbContext.SaveChangesAsync();

        var result = await _queries.GetByExternalIdAsync("ext-123");

        result.Should().NotBeNull();
        result!.ExternalTransactionId.Should().Be("ext-123");
    }

    [Fact]
    public async Task GetByExternalIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        var result = await _queries.GetByExternalIdAsync("non-existent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByExternalIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _queries.GetByExternalIdAsync(""));
    }
}
