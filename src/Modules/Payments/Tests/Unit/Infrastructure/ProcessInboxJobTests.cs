using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

public class ProcessInboxJobTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ProcessInboxJob>> _loggerMock;
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentTransactionRepository> _paymentTransactionRepositoryMock;
    private readonly ProcessInboxJob _job;

    public ProcessInboxJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ProcessInboxJob>>();
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _paymentTransactionRepositoryMock = new Mock<IPaymentTransactionRepository>();
        _job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSessionCompleted_ShouldActivateSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalSubId = "sub_123";
        var customerId = "cus_123";
        var data = new StripeEventData(
            "checkout.session.completed",
            "evt_1",
            externalSubId,
            customerId,
            providerId);

        var subscription = new Subscription(providerId, "plan", Money.FromDecimal(10));
        
        _repositoryMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        await _job.ProcessStripeEventAsync(data, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalSubId);
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_ShouldRenewSubscription()
    {
        // Arrange
        var externalSubId = "sub_123";
        var nextPeriodEnd = DateTime.UtcNow.AddMonths(1);
        var data = new StripeEventData(
            "invoice.paid",
            "evt_2",
            externalSubId,
            "cus_123",
            null,
            nextPeriodEnd,
            9990,
            "brl",
            "in_1");

        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddDays(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        await _job.ProcessStripeEventAsync(data, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        subscription.ExpiresAt.Should().Be(nextPeriodEnd);
        _paymentTransactionRepositoryMock.Verify(x => x.AddAsync(It.Is<PaymentTransaction>(t => t.ExternalTransactionId == "in_1"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WhenCurrencyDiverges_ShouldLogWarning()
    {
        // Arrange
        var externalSubId = "sub_123";
        var data = new StripeEventData(
            "invoice.paid", "evt_2", externalSubId, "cus_123", null, 
            DateTime.UtcNow.AddMonths(1), 1000, "usd", "in_1");

        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10, "BRL"));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddDays(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        await _job.ProcessStripeEventAsync(data, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Currency divergence")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_SubscriptionDeleted_ShouldCancelSubscription()
    {
        // Arrange
        var externalSubId = "sub_123";
        var data = new StripeEventData("customer.subscription.deleted", "evt_3", externalSubId, "cus_123", null);

        var subscription = new Subscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddMonths(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        // Act
        await _job.ProcessStripeEventAsync(data, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_WhenEssentialDataMissing_ShouldThrow()
    {
        // Arrange
        var data = new StripeEventData("checkout.session.completed", "evt_err", null, null, null);

        // Act
        var act = () => _job.ProcessStripeEventAsync(data, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Essential data missing*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_WhenSubscriptionNotFound_ShouldThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var data = new StripeEventData("checkout.session.completed", "evt_4", "sub_4", "cus_4", providerId);
        
        _repositoryMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        // Act
        var act = () => _job.ProcessStripeEventAsync(data, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Subscription not found*");
    }
}
