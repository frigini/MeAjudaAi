using MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Queries;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers.Subscriptions.Queries;

public class GetActiveSubscriptionByProviderQueryHandlerTests
{
    private readonly Mock<ISubscriptionQueries> _subscriptionQueriesMock;
    private readonly GetActiveSubscriptionByProviderQueryHandler _handler;

    public GetActiveSubscriptionByProviderQueryHandlerTests()
    {
        _subscriptionQueriesMock = new Mock<ISubscriptionQueries>();
        _handler = new GetActiveSubscriptionByProviderQueryHandler(_subscriptionQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WithSubscription_WhenExists()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var subscription = new Subscription(providerId, "plan", Money.FromDecimal(10, "BRL"));
        
        _subscriptionQueriesMock.Setup(q => q.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscription);

        var query = new GetActiveSubscriptionByProviderQuery(providerId, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(subscription);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSuccess_WithNull_WhenDoesNotExist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        
        _subscriptionQueriesMock.Setup(q => q.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var query = new GetActiveSubscriptionByProviderQuery(providerId, Guid.NewGuid());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }
}
