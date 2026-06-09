using FluentAssertions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Events.Handlers;

public class SubscriptionIntegrationEventHandlersTests : IDisposable
{
    private readonly Mock<ILogger<SubscriptionActivatedIntegrationEventHandler>> _mockLoggerActivated;
    private readonly Mock<ILogger<SubscriptionCanceledIntegrationEventHandler>> _mockLoggerCanceled;
    private readonly Mock<ILogger<SubscriptionExpiredIntegrationEventHandler>> _mockLoggerExpired;
    private readonly Mock<ILogger<SubscriptionRenewedIntegrationEventHandler>> _mockLoggerRenewed;
    private readonly PaymentsDbContext _context;

    public SubscriptionIntegrationEventHandlersTests()
    {
        _mockLoggerActivated = new Mock<ILogger<SubscriptionActivatedIntegrationEventHandler>>();
        _mockLoggerCanceled = new Mock<ILogger<SubscriptionCanceledIntegrationEventHandler>>();
        _mockLoggerExpired = new Mock<ILogger<SubscriptionExpiredIntegrationEventHandler>>();
        _mockLoggerRenewed = new Mock<ILogger<SubscriptionRenewedIntegrationEventHandler>>();

        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new PaymentsDbContext(options, null!);
    }

    [Fact]
    public async Task SubscriptionActivated_UpdatesStatusToActive()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", new Shared.Domain.ValueObjects.Money(10));
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var handler = new SubscriptionActivatedIntegrationEventHandler(_context, _mockLoggerActivated.Object);
        var evt = new SubscriptionActivatedIntegrationEvent("Test", subscription.Id, Guid.NewGuid());

        await handler.HandleAsync(evt);

        var updated = await _context.Subscriptions.FindAsync(subscription.Id);
        updated!.Status.Should().Be(ESubscriptionStatus.Active);
    }

    [Fact]
    public async Task SubscriptionCanceled_UpdatesStatusToCanceled()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", new Shared.Domain.ValueObjects.Money(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddMonths(1));
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var handler = new SubscriptionCanceledIntegrationEventHandler(_context, _mockLoggerCanceled.Object);
        var evt = new SubscriptionCanceledIntegrationEvent("Test", subscription.Id);

        await handler.HandleAsync(evt);

        var updated = await _context.Subscriptions.FindAsync(subscription.Id);
        updated!.Status.Should().Be(ESubscriptionStatus.Canceled);
    }

    [Fact]
    public async Task SubscriptionExpired_UpdatesStatusToExpired()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", new Shared.Domain.ValueObjects.Money(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddMonths(1));
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var handler = new SubscriptionExpiredIntegrationEventHandler(_context, _mockLoggerExpired.Object);
        var evt = new SubscriptionExpiredIntegrationEvent("Test", subscription.Id);

        await handler.HandleAsync(evt);

        var updated = await _context.Subscriptions.FindAsync(subscription.Id);
        updated!.Status.Should().Be(ESubscriptionStatus.Expired);
    }

    [Fact]
    public async Task SubscriptionRenewed_UpdatesStatusToActive()
    {
        var subscription = new Subscription(Guid.NewGuid(), "plan", new Shared.Domain.ValueObjects.Money(10));
        subscription.Activate("sub", "cus", DateTime.UtcNow.AddMonths(1));
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        var handler = new SubscriptionRenewedIntegrationEventHandler(_context, _mockLoggerRenewed.Object);
        var newExpiration = DateTime.UtcNow.AddMonths(2);
        var evt = new SubscriptionRenewedIntegrationEvent("Test", subscription.Id, Guid.NewGuid(), newExpiration);

        await handler.HandleAsync(evt);

        var updated = await _context.Subscriptions.FindAsync(subscription.Id);
        updated!.Status.Should().Be(ESubscriptionStatus.Active);
        updated.ExpiresAt.Should().BeCloseTo(newExpiration, TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
