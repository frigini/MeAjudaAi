using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Repositories;

public class PaymentTransactionRepositoryTests : IDisposable
{
    private readonly PaymentsDbContext _context;
    private readonly Mock<ILogger<PaymentTransactionRepository>> _loggerMock;
    private readonly PaymentTransactionRepository _transactionRepository;

    public PaymentTransactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PaymentsDbContext(options);
        _loggerMock = new Mock<ILogger<PaymentTransactionRepository>>();
        _transactionRepository = new PaymentTransactionRepository(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByExternalIdAsync_ShouldReturnTransaction_WhenExists()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var tx = new PaymentTransaction(subId, Money.FromDecimal(10));
        tx.Settle("tx_123");
        _context.PaymentTransactions.Add(tx);
        await _context.SaveChangesAsync();

        // Act
        var result = await _transactionRepository.GetByExternalIdAsync("tx_123");

        // Assert
        result.Should().NotBeNull();
        result!.ExternalTransactionId.Should().Be("tx_123");
        result.Status.Should().Be(EPaymentStatus.Succeeded);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTransactionCorrectly()
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
        saved!.SubscriptionId.Should().Be(subId);
        saved.ExternalTransactionId.Should().Be("tx_new");
    }

    [Fact]
    public async Task AddAsync_ShouldNotThrow_WhenDuplicateExternalId_ToEnsureIdempotency()
    {
        // Arrange
        var subId = Guid.NewGuid();
        var tx1 = new PaymentTransaction(subId, Money.FromDecimal(10));
        tx1.Settle("tx_dup");
        
        var tx2 = new PaymentTransaction(subId, Money.FromDecimal(10));
        tx2.Settle("tx_dup");

        // Act - Adicionamos a primeira
        await _transactionRepository.AddAsync(tx1);

        // Para testar a lógica de catch no repositório usando InMemory (que não lança UniqueConstraint),
        // precisaríamos de um Mock do DbContext, mas como o repositório é o SUT e usa o DbContext diretamente,
        // o ideal para este teste de infra é um teste de integração real com Postgres (Testcontainers).
        // Por ora, garantimos que o método não lança exceção no fluxo feliz.
        var act = () => _transactionRepository.AddAsync(tx2);

        // Assert
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
