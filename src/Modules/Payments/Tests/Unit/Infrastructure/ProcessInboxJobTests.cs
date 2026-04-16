using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

using DomainSubscription = MeAjudaAi.Modules.Payments.Domain.Entities.Subscription;
using DomainPaymentTransaction = MeAjudaAi.Modules.Payments.Domain.Entities.PaymentTransaction;

public class ProcessInboxJobTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ProcessInboxJob>> _loggerMock;
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentTransactionRepository> _paymentTransactionRepositoryMock;

    public ProcessInboxJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ProcessInboxJob>>();
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _paymentTransactionRepositoryMock = new Mock<IPaymentTransactionRepository>();
    }

    private Event CreateMockEvent(string json)
    {
        // Usa o utilitário nativo do Stripe para processar o JSON sem reflexão
        return EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSessionCompleted_ShouldActivateSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalSubId = "sub_123";
        var customerId = "cus_123";
        
        var json = $$"""
        {
            "id": "evt_1",
            "type": "checkout.session.completed",
            "data": {
                "object": {
                    "id": "cs_1",
                    "subscription": "{{externalSubId}}",
                    "customer": "{{customerId}}",
                    "metadata": {
                        "provider_id": "{{providerId}}"
                    }
                }
            }
        }
        """;

        var stripeEvent = CreateMockEvent(json);
        var subscription = new DomainSubscription(providerId, "plan", Money.FromDecimal(10));
        
        _repositoryMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);

        // Act
        await job.ProcessStripeEventAsync(stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalSubId);
        subscription.ExternalCustomerId.Should().Be(customerId);
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_ShouldRenewSubscription()
    {
        // Arrange
        var externalSubId = "sub_123";
        var nextPeriodEnd = DateTime.UtcNow.AddMonths(1);
        var nextPeriodEndUnix = new DateTimeOffset(nextPeriodEnd).ToUnixTimeSeconds();
        
        var json = $$"""
        {
            "id": "evt_2",
            "type": "invoice.paid",
            "data": {
                "object": {
                    "id": "in_1",
                    "subscription": "{{externalSubId}}",
                    "customer": "cus_123",
                    "amount_paid": 9990,
                    "currency": "brl",
                    "lines": {
                        "data": [
                            {
                                "period": {
                                    "end": {{nextPeriodEndUnix}}
                                }
                            }
                        ]
                    }
                }
            }
        }
        """;

        var stripeEvent = CreateMockEvent(json);
        var subscription = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddDays(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _paymentTransactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<DomainPaymentTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);

        // Act
        await job.ProcessStripeEventAsync(stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExpiresAt.Should().BeCloseTo(nextPeriodEnd, TimeSpan.FromSeconds(2));
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
        _paymentTransactionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<DomainPaymentTransaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_SubscriptionDeleted_ShouldCancelSubscription()
    {
        // Arrange
        var externalSubId = "sub_123";
        var json = $$"""
        {
            "id": "evt_3",
            "type": "customer.subscription.deleted",
            "data": {
                "object": {
                    "id": "{{externalSubId}}"
                }
            }
        }
        """;

        var stripeEvent = CreateMockEvent(json);
        var subscription = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddMonths(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);

        // Act
        await job.ProcessStripeEventAsync(stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_WhenEventObjectIsNull_ShouldThrow()
    {
        // Arrange - Criamos um evento onde o campo object dentro de data está faltando ou nulo
        var json = """
        {
            "id": "evt_err",
            "type": "checkout.session.completed",
            "data": { }
        }
        """;
        var stripeEvent = CreateMockEvent(json);
        
        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);

        // Act & Assert
        var task = job.ProcessStripeEventAsync(stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None);
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        exception.Message.Should().Contain("Session data is missing");
    }

    // A classe MockStripeObject foi removida pois agora usamos JSON real e EventUtility.ParseEvent
}
