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
using System.Reflection;
using DomainSubscription = MeAjudaAi.Modules.Payments.Domain.Entities.Subscription;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure;

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

    private Event CreateMockEvent(string type, object dataObject)
    {
        var stripeEvent = new Event { Type = type, Id = "evt_mock" };
        var eventData = new EventData();
        
        // Force the object into the event data via reflection
        typeof(EventData).GetProperty("Object")?.SetValue(eventData, dataObject);
        typeof(Event).GetProperty("Data")?.SetValue(stripeEvent, eventData);
        
        return stripeEvent;
    }

    [Fact]
    public async Task ProcessStripeEventAsync_CheckoutSessionCompleted_ShouldActivateSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalSubId = "sub_123";
        var customerId = "cus_123";
        
        // Use reflection to create a session-like object that the handler can cast to dynamic
        var session = new MockStripeObject();
        session.Set("Id", "cs_1");
        session.Set("SubscriptionId", externalSubId);
        session.Set("CustomerId", customerId);
        session.Set("Metadata", new Dictionary<string, string> { { "provider_id", providerId.ToString() } });

        var stripeEvent = CreateMockEvent("checkout.session.completed", session);
        var subscription = new DomainSubscription(providerId, "plan", Money.FromDecimal(10));
        
        _repositoryMock.Setup(x => x.GetLatestByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);
        var method = typeof(ProcessInboxJob).GetMethod("ProcessStripeEventAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        await (Task)method!.Invoke(job, new object[] { stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None })!;

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExternalSubscriptionId.Should().Be(externalSubId);
        subscription.ExternalCustomerId.Should().Be(customerId);
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Skip = "Needs review - PaymentTransaction creation needs IPaymentTransactionRepository setup in handler")]
    public async Task ProcessStripeEventAsync_InvoicePaid_ShouldRenewSubscription()
    {
        // Arrange
        var externalSubId = "sub_123";
        var nextPeriodEnd = DateTime.UtcNow.AddMonths(1);
        
        var invoice = new MockStripeObject();
        invoice.Set("Id", "in_1");
        invoice.Set("SubscriptionId", externalSubId);
        
        var period = new MockStripeObject();
        period.Set("End", nextPeriodEnd);
        
        var line = new MockStripeObject();
        line.Set("Period", period);
        
        var lines = new MockStripeObject();
        lines.Set("Data", new List<object> { line });
        
        invoice.Set("Lines", lines);

        var stripeEvent = CreateMockEvent("invoice.paid", invoice);
        var subscription = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddDays(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        _paymentTransactionRepositoryMock.Setup(x => x.AddAsync(It.IsAny<MeAjudaAi.Modules.Payments.Domain.Entities.PaymentTransaction>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);
        var method = typeof(ProcessInboxJob).GetMethod("ProcessStripeEventAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        await (Task)method!.Invoke(job, new object[] { stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None })!;

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Active);
        subscription.ExpiresAt.Should().BeCloseTo(nextPeriodEnd, TimeSpan.FromSeconds(1));
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_SubscriptionDeleted_ShouldCancelSubscription()
    {
        // Arrange
        var externalSubId = "sub_123";
        var stripeSubscription = new MockStripeObject();
        stripeSubscription.Set("Id", externalSubId);

        var stripeEvent = CreateMockEvent("customer.subscription.deleted", stripeSubscription);
        var subscription = new DomainSubscription(Guid.NewGuid(), "plan", Money.FromDecimal(10));
        subscription.Activate(externalSubId, "cus_123", DateTime.UtcNow.AddMonths(1));
        
        _repositoryMock.Setup(x => x.GetByExternalIdAsync(externalSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);
        var method = typeof(ProcessInboxJob).GetMethod("ProcessStripeEventAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        await (Task)method!.Invoke(job, new object[] { stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None })!;

        // Assert
        subscription.Status.Should().Be(ESubscriptionStatus.Canceled);
        _repositoryMock.Verify(x => x.UpdateAsync(subscription, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessStripeEventAsync_WhenEventObjectIsNull_ShouldThrow()
    {
        // Arrange
        var stripeEvent = new Event { Type = "checkout.session.completed", Data = new EventData { Object = null } };
        var job = new ProcessInboxJob(_serviceProviderMock.Object, _loggerMock.Object);
        var method = typeof(ProcessInboxJob).GetMethod("ProcessStripeEventAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var act = () => method!.Invoke(job, new object[] { stripeEvent, _repositoryMock.Object, _paymentTransactionRepositoryMock.Object, CancellationToken.None }) as Task;

        // Assert
        var task = act();
        Assert.NotNull(task);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task!);
        exception.Message.Should().Contain("Session data is missing");
    }

    // A class that session/invoice/etc will be cast to dynamic
    private class MockStripeObject : System.Dynamic.DynamicObject, IHasObject
    {
        private readonly Dictionary<string, object> _properties = new();
        public void Set(string name, object value) => _properties[name] = value;
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object? result)
            => _properties.TryGetValue(binder.Name, out result);

        // Explicitly implement IHasObject to satisfy the cast if needed
        public string Object { get; set; } = "mock";
    }
}
