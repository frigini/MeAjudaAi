using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Commands;
using MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers.Subscriptions.Commands;

public class CreateBillingPortalSessionCommandHandlerTests
{
    private readonly Mock<IQueryDispatcher> _queryDispatcherMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<ILogger<CreateBillingPortalSessionCommandHandler>> _loggerMock;
    private readonly CreateBillingPortalSessionCommandHandler _handler;

    public CreateBillingPortalSessionCommandHandlerTests()
    {
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _loggerMock = new Mock<ILogger<CreateBillingPortalSessionCommandHandler>>();

        _handler = new CreateBillingPortalSessionCommandHandler(
            _queryDispatcherMock.Object, 
            _gatewayMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPortalUrl_WhenValid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalCustomerId = "cus_123";
        var returnUrl = "https://meajudaai.com/account";
        var expectedUrl = "https://stripe.com/portal/123";

        var sub = new Subscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate("sub_123", externalCustomerId, DateTime.UtcNow.AddMonths(1));

        _queryDispatcherMock.Setup(x => x.QueryAsync<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>(It.IsAny<GetActiveSubscriptionByProviderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Subscription?>.Success(sub));

        _gatewayMock.Setup(x => x.CreateBillingPortalSessionAsync(externalCustomerId, returnUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var command = new CreateBillingPortalSessionCommand(providerId, returnUrl);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenNoActiveSubscription()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _queryDispatcherMock.Setup(x => x.QueryAsync<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>(It.IsAny<GetActiveSubscriptionByProviderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Subscription?>.Success(null));

        var command = new CreateBillingPortalSessionCommand(providerId, "https://meajudaai.com/account");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowBusinessRule_WhenMissingCustomerId()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var sub = new Subscription(providerId, "plan", Money.FromDecimal(10));

        _queryDispatcherMock.Setup(x => x.QueryAsync<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>(It.IsAny<GetActiveSubscriptionByProviderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Subscription?>.Success(sub));

        var command = new CreateBillingPortalSessionCommand(providerId, "https://meajudaai.com/account");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>().Where(e => e.RuleName == "MISSING_EXTERNAL_CUSTOMER_ID");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowBusinessRule_WhenGatewayFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var sub = new Subscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate("sub_123", "cus_123", DateTime.UtcNow.AddMonths(1));

        _queryDispatcherMock.Setup(x => x.QueryAsync<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>(It.IsAny<GetActiveSubscriptionByProviderQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Subscription?>.Success(sub));

        _gatewayMock.Setup(x => x.CreateBillingPortalSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var command = new CreateBillingPortalSessionCommand(providerId, "https://meajudaai.com/account");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>().Where(e => e.RuleName == "GATEWAY_SESSION_FAILURE");
    }
}
