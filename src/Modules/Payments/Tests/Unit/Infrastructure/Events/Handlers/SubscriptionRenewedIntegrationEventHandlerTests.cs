using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Events.Handlers;

public class SubscriptionRenewedIntegrationEventHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Subscription, Guid>> _repositoryMock;
    private readonly Mock<ILogger<SubscriptionRenewedIntegrationEventHandler>> _loggerMock;
    private readonly SubscriptionRenewedIntegrationEventHandler _handler;

    public SubscriptionRenewedIntegrationEventHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Subscription, Guid>>();
        _loggerMock = new Mock<ILogger<SubscriptionRenewedIntegrationEventHandler>>();

        _uowMock.Setup(u => u.GetRepository<Subscription, Guid>()).Returns(_repositoryMock.Object);

        _handler = new SubscriptionRenewedIntegrationEventHandler(
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenValidEvent_ShouldRenewSubscription()
    {
        // Arrange
        var subscriptionId = Guid.NewGuid();
        var subscription = new Subscription(Guid.NewGuid(), "gold", new MeAjudaAi.Shared.Domain.ValueObjects.Money(100, "BRL"));
        subscription.GetType().GetProperty("Status")?.SetValue(subscription, MeAjudaAi.Modules.Payments.Domain.Enums.ESubscriptionStatus.Active);
        
        _repositoryMock.Setup(r => r.TryFindAsync(subscriptionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var integrationEvent = new SubscriptionRenewedIntegrationEvent("Payments", subscriptionId, Guid.NewGuid(), DateTime.UtcNow.AddMonths(1));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        subscription.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
