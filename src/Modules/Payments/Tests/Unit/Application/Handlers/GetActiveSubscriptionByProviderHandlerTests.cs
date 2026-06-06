using FluentAssertions;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.ValueObjects;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

public class GetActiveSubscriptionByProviderHandlerTests
{
    private readonly Mock<ISubscriptionQueries> _subscriptionQueriesMock;
    private readonly GetActiveSubscriptionByProviderHandler _handler;

    public GetActiveSubscriptionByProviderHandlerTests()
    {
        _subscriptionQueriesMock = new Mock<ISubscriptionQueries>();
        _handler = new GetActiveSubscriptionByProviderHandler(_subscriptionQueriesMock.Object);
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
