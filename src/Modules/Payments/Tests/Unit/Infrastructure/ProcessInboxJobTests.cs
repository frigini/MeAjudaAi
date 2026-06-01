using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using Stripe;
using DomainSubscription = MeAjudaAi.Modules.Payments.Domain.Entities.Subscription;
using DomainPaymentTransaction = MeAjudaAi.Modules.Payments.Domain.Entities.PaymentTransaction;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

public class ProcessInboxJobTests : IDisposable
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ProcessInboxJob>> _loggerMock;
    private readonly Mock<ISubscriptionQueries> _subscriptionQueriesMock;
    private readonly SqliteConnection _connection;
    private readonly PaymentsDbContext _dbContext;
    private readonly ProcessInboxJob _job;

    public ProcessInboxJobTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ProcessInboxJob>>();
        _subscriptionQueriesMock = new Mock<ISubscriptionQueries>();

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PaymentsDbContext(options);
        _dbContext.Database.EnsureCreated();

        _job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void MapToStripeEventData_CheckoutSessionCompleted_ShouldMapCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var json = $$"""
        {
            "id": "evt_1",
            "type": "checkout.session.completed",
            "data": {
                "object": {
                    "object": "checkout.session",
                    "id": "cs_1",
                    "subscription": "sub_1",
                    "customer": "cus_1",
                    "metadata": { "provider_id": "{{providerId}}" }
                }
            }
        }
        """;
        var stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);

        // Act
        var result = _job.MapToStripeEventData(stripeEvent);

        // Assert
        result.Type.Should().Be("checkout.session.completed");
        result.SubscriptionId.Should().Be("sub_1");
        result.CustomerId.Should().Be("cus_1");
        result.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public void MapToStripeEventData_InvoicePaid_ShouldMapCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "evt_2",
            "type": "invoice.paid",
            "data": {
                "object": {
                    "object": "invoice",
                    "id": "in_1",
                    "customer": "cus_1",
                    "amount_paid": 5000,
                    "currency": "brl",
                    "parent": {
                        "subscription_details": {
                            "subscription": {
                                "id": "sub_2"
                            }
                        }
                    },
                    "lines": {
                        "data": [{ "subscription": "sub_2", "period": { "end": 1740000000 } }]
                    }
                }
            }
        }
        """;
        var stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);

        // Act
        var result = _job.MapToStripeEventData(stripeEvent);

        // Assert
        result.Type.Should().Be("invoice.paid");
        result.SubscriptionId.Should().Be("sub_2");
        result.AmountPaid.Should().Be(5000);
        result.Currency.Should().Be("brl");
        result.InvoiceId.Should().Be("in_1");
        result.PeriodEnd.Should().NotBeNull();
    }

    [Fact]
    public void MapToStripeEventData_SubscriptionDeleted_ShouldMapCorrectly()
    {
        // Arrange
        var json = """
        {
            "id": "evt_3",
            "type": "customer.subscription.deleted",
            "data": {
                "object": {
                    "object": "subscription",
                    "id": "sub_3",
                    "customer": "cus_3"
                }
            }
        }
        """;
        var stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);

        // Act
        var result = _job.MapToStripeEventData(stripeEvent);

        // Assert
        result.Type.Should().Be("customer.subscription.deleted");
        result.SubscriptionId.Should().Be("sub_3");
        result.CustomerId.Should().Be("cus_3");
    }

    [Fact]
    public void MapToStripeEventData_UnknownEvent_ShouldReturnEmptyData()
    {
        // Arrange
        var json = """
        {
            "id": "evt_4",
            "type": "account.updated",
            "data": { "object": { "object": "account" } }
        }
        """;
        var stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);

        // Act
        var result = _job.MapToStripeEventData(stripeEvent);

        // Assert
        result.Type.Should().Be("account.updated");
        result.SubscriptionId.Should().BeNull();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSession_WhenSubscriptionNotFound_ShouldThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var data = new StripeEventData("checkout.session.completed", "evt_1", "sub_1", "cus_1", providerId);
        
        _subscriptionQueriesMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription?)null);

        // Act
        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Subscription not found*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WhenSubscriptionNotFound_ShouldThrow()
    {
        // Arrange
        var data = new StripeEventData("invoice.paid", "evt_2", "sub_not_found", "cus_1", null);
        
        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync("sub_not_found", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription?)null);

        // Act
        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_WhenCustomerIdMissing_ShouldThrow()
    {
        // Arrange
        var data = new StripeEventData("checkout.session.completed", "evt_1", "sub_1", null, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        // Subscription.Activate handles it by throwing ArgumentException/InvalidOperation or similar
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WhenInvoiceIdMissing_ShouldSkipTransaction()
    {
        // Arrange
        var externalSubId = "sub_123";
        var data = new StripeEventData("invoice.paid", "evt_2", externalSubId, "cus_1", null, null, 100, "brl", null);
        var sub = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        sub.Activate(externalSubId, "cus_1", DateTime.UtcNow.AddDays(1));

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        _dbContext.PaymentTransactions.Local.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WithCurrencyDivergence_ShouldLogWarningAndSucceed()
    {
        // Arrange
        var externalSubId = "sub_123";
        var data = new StripeEventData("invoice.paid", "evt_2", externalSubId, "cus_1", null, null, 10000, "usd", "in_1");
        var sub = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(100, "BRL"));
        sub.Activate(externalSubId, "cus_1", DateTime.UtcNow.AddDays(1));

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        sub.Status.Should().Be(ESubscriptionStatus.Active);
        _dbContext.PaymentTransactions.Local.Should().ContainSingle();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSession_WhenProviderIdMissing_ShouldThrow()
    {
        // Arrange
        var data = new StripeEventData("checkout.session.completed", "evt_1", "sub_1", "cus_1", null);

        // Act
        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Essential data missing*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSession_WhenAlreadyActiveWithSameIds_ShouldSkip()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var subId = "sub_123";
        var data = new StripeEventData("checkout.session.completed", "evt_1", subId, "cus_1", providerId);
        var sub = new DomainSubscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate(subId, "cus_1", DateTime.UtcNow.AddDays(1));

        _subscriptionQueriesMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert - no changes expected, subscription was already active with same IDs
        sub.ExternalSubscriptionId.Should().Be(subId);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_SubscriptionDeleted_WhenNotFound_ShouldThrow()
    {
        // Arrange
        var externalSubId = "sub_missing";
        var data = new StripeEventData("customer.subscription.deleted", "evt_3", externalSubId, "cus_3", null);

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription?)null);

        // Act
        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_UnknownEventType_ShouldReturnWithoutThrow()
    {
        // Arrange
        var data = new StripeEventData("unknown.event", "evt_999", null, null, null);

        // Act
        Func<Task> act = () => _job.ProcessStripeEventAsync(data, _dbContext, _subscriptionQueriesMock.Object, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
