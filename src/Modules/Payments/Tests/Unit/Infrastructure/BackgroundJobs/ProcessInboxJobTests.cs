using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs;
using MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs.Models;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stripe;
using DomainSubscription = MeAjudaAi.Modules.Payments.Domain.Entities.Subscription;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.BackgroundJobs;

public class ProcessInboxJobTests : BaseSqliteInMemoryDatabaseTest<PaymentsDbContext>
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ProcessInboxJob>> _loggerMock;
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly Mock<ISubscriptionQueries> _subscriptionQueriesMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly ProcessInboxJob _job;

    public ProcessInboxJobTests()
        : base(options => new PaymentsDbContext(options))
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<ProcessInboxJob>>();
        _subscriptionQueriesMock = new Mock<ISubscriptionQueries>();
        _messageBusMock = new Mock<IMessageBus>();

        _job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);

        _serviceProvider = BuildServiceProvider(services =>
        {
            services.AddSingleton(DbContext);
            services.AddSingleton(_subscriptionQueriesMock.Object);
            services.AddSingleton(_messageBusMock.Object);
            services.AddKeyedSingleton<IUnitOfWork>(ModuleKeys.Payments, DbContext);
        });
    }

    #region MapToStripeEventData

    [Fact]
    public void MapToStripeEventData_CheckoutSessionCompleted_ShouldMapCorrectly()
    {
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

        var result = _job.MapToStripeEventData(stripeEvent);

        result.Type.Should().Be("checkout.session.completed");
        result.SubscriptionId.Should().Be("sub_1");
        result.CustomerId.Should().Be("cus_1");
        result.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public void MapToStripeEventData_InvoicePaid_ShouldMapCorrectly()
    {
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

        var result = _job.MapToStripeEventData(stripeEvent);

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

        var result = _job.MapToStripeEventData(stripeEvent);

        result.Type.Should().Be("customer.subscription.deleted");
        result.SubscriptionId.Should().Be("sub_3");
        result.CustomerId.Should().Be("cus_3");
    }

    [Fact]
    public void MapToStripeEventData_UnknownEvent_ShouldReturnEmptyData()
    {
        var json = """
        {
            "id": "evt_4",
            "type": "account.updated",
            "data": { "object": { "object": "account" } }
        }
        """;
        var stripeEvent = EventUtility.ParseEvent(json, throwOnApiVersionMismatch: false);

        var result = _job.MapToStripeEventData(stripeEvent);

        result.Type.Should().Be("account.updated");
        result.SubscriptionId.Should().BeNull();
    }

    #endregion

    #region ProcessStripeEventAsync

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSession_WhenSubscriptionNotFound_ShouldThrow()
    {
        var providerId = Guid.NewGuid();
        var data = new StripeEventData("checkout.session.completed", "evt_1", "sub_1", "cus_1", providerId);

        _subscriptionQueriesMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription?)null);

        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Subscription not found*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WhenSubscriptionNotFound_ShouldThrow()
    {
        var data = new StripeEventData("invoice.paid", "evt_2", "sub_not_found", "cus_1", null);

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync("sub_not_found", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription?)null);

        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_WhenCustomerIdMissing_ShouldThrow()
    {
        var data = new StripeEventData("checkout.session.completed", "evt_1", "sub_1", null, Guid.NewGuid());

        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WhenInvoiceIdMissing_ShouldSkipTransaction()
    {
        var externalSubId = "sub_123";
        var data = new StripeEventData("invoice.paid", "evt_2", externalSubId, "cus_1", null, null, 100, "brl", null);
        var sub = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        sub.Activate(externalSubId, "cus_1", DateTime.UtcNow.AddDays(1));

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        DbContext.PaymentTransactions.Local.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_InvoicePaid_WithCurrencyDivergence_ShouldLogWarningAndSucceed()
    {
        var externalSubId = "sub_123";
        var data = new StripeEventData("invoice.paid", "evt_2", externalSubId, "cus_1", null, null, 10000, "usd", "in_1");
        var sub = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(100, "BRL"));
        sub.Activate(externalSubId, "cus_1", DateTime.UtcNow.AddDays(1));

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        sub.Status.Should().Be(ESubscriptionStatus.Active);
        DbContext.PaymentTransactions.Local.Should().ContainSingle();
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSession_WhenProviderIdMissing_ShouldThrow()
    {
        var data = new StripeEventData("checkout.session.completed", "evt_1", "sub_1", "cus_1", null);

        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Essential data missing*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSession_WhenAlreadyActiveWithSameIds_ShouldSkip()
    {
        var providerId = Guid.NewGuid();
        var subId = "sub_123";
        var data = new StripeEventData("checkout.session.completed", "evt_1", subId, "cus_1", providerId);
        var sub = new DomainSubscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate(subId, "cus_1", DateTime.UtcNow.AddDays(1));

        _subscriptionQueriesMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        sub.ExternalSubscriptionId.Should().Be(subId);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_SubscriptionDeleted_WhenNotFound_ShouldThrow()
    {
        var externalSubId = "sub_missing";
        var data = new StripeEventData("customer.subscription.deleted", "evt_3", externalSubId, "cus_3", null);

        _subscriptionQueriesMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainSubscription?)null);

        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, DbContext, _subscriptionQueriesMock.Object, DbContext, DbContext, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task ProcessStripeEventAsync_UnknownEventType_ShouldReturnWithoutThrow()
    {
        var data = new StripeEventData("unknown.event", "evt_999", null, null, null);

        Func<Task> act = async () => await _job.ProcessStripeEventAsync(data, new Mock<IRepository<PaymentTransaction, Guid>>().Object, _subscriptionQueriesMock.Object, DbContext, new Mock<IUnitOfWork>().Object, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region ExecuteAsync (integration via wrapper)

    [Fact]
    public async Task ExecuteAsync_ShouldProcessMessages_WhenTheyExist()
    {
        var content = "{\"id\": \"evt_1\", \"type\": \"unknown\", \"data\": {\"object\": { \"object\": \"account\" }}}";
        var message = new InboxMessage("unknown", content, "evt_1");
        DbContext.InboxMessages.Add(message);
        await DbContext.SaveChangesAsync();

        var wrapper = new ProcessInboxJobWrapper(_serviceProvider, _loggerMock.Object);

        await wrapper.DoExecuteStepAsync(CancellationToken.None);

        var processedMessage = await DbContext.InboxMessages.FindAsync(message.Id);
        processedMessage!.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateEvent_ShouldSkip()
    {
        var content = "{\"id\": \"evt_1\", \"type\": \"checkout.session.completed\", \"data\": {\"object\": {\"id\": \"sub_123\"}}}";

        var message = new InboxMessage("checkout.session.completed", content, "evt_1");
        message.MarkAsProcessed();
        DbContext.InboxMessages.Add(message);
        await DbContext.SaveChangesAsync();

        var newMessage = new InboxMessage("checkout.session.completed", content, "evt_2");
        DbContext.InboxMessages.Add(newMessage);
        await DbContext.SaveChangesAsync();

        var wrapper = new ProcessInboxJobWrapper(_serviceProvider, _loggerMock.Object);

        await wrapper.DoExecuteStepAsync(CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(It.IsAny<object>(), null, It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _serviceProvider?.Dispose();
        }
        base.Dispose(disposing);
    }

    private class ProcessInboxJobWrapper(IServiceProvider serviceProvider, ILogger<ProcessInboxJob> logger)
        : ProcessInboxJob(serviceProvider, logger)
    {
        public async Task DoExecuteStepAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var subscriptionQueries = scope.ServiceProvider.GetRequiredService<ISubscriptionQueries>();
            var uow = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(ModuleKeys.Payments);

            var messages = await dbContext.InboxMessages
                .Where(m => m.ProcessedAt == null && m.RetryCount < m.MaxRetries && (m.NextAttemptAt == null || m.NextAttemptAt <= DateTime.UtcNow))
                .OrderBy(m => m.CreatedAt)
                .Take(20)
                .ToListAsync(ct);

            if (messages.Count == 0) return;

            await ProcessMessagesBatchAsync(messages, dbContext, subscriptionQueries, uow, ct);
            await uow.SaveChangesAsync(ct);
        }
    }
}
