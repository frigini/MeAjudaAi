using MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;
using MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Application.Handlers;

public class GetBillingPortalCommandHandlerTests
{
    private readonly Mock<ISubscriptionRepository> _repositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<GetBillingPortalCommandHandler>> _loggerMock;
    private readonly GetBillingPortalCommandHandler _handler;

    public GetBillingPortalCommandHandlerTests()
    {
        _repositoryMock = new Mock<ISubscriptionRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<GetBillingPortalCommandHandler>>();
        _handler = new GetBillingPortalCommandHandler(
            _repositoryMock.Object, 
            _gatewayMock.Object, 
            _configurationMock.Object, 
            _loggerMock.Object);

        // Setup default allowed hosts
        var sectionMock = new Mock<IConfigurationSection>();
        var childMock = new Mock<IConfigurationSection>();
        childMock.Setup(x => x.Value).Returns("localhost");
        sectionMock.Setup(x => x.GetChildren()).Returns(new[] { childMock.Object });
        _configurationMock.Setup(x => x.GetSection("Payments:AllowedReturnHosts")).Returns(sectionMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnPortalUrl_WhenValid()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var externalCustomerId = "cus_123";
        var returnUrl = "https://localhost/account";
        var expectedUrl = "https://stripe.com/portal/123";

        var sub = new Subscription(providerId, "plan", Money.FromDecimal(10));
        sub.Activate("sub_123", externalCustomerId, DateTime.UtcNow.AddMonths(1));

        _repositoryMock.Setup(x => x.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        _gatewayMock.Setup(x => x.CreateBillingPortalSessionAsync(externalCustomerId, returnUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var command = new GetBillingPortalCommand(providerId, returnUrl);

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
        _repositoryMock.Setup(x => x.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var command = new GetBillingPortalCommand(providerId, "https://localhost/account");

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
        // Note: sub is not activated, so ExternalCustomerId is null

        _repositoryMock.Setup(x => x.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        var command = new GetBillingPortalCommand(providerId, "https://localhost/account");

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

        _repositoryMock.Setup(x => x.GetActiveByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        _gatewayMock.Setup(x => x.CreateBillingPortalSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var command = new GetBillingPortalCommand(providerId, "https://localhost/account");

        // Act
        var act = () => _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>().Where(e => e.RuleName == "GATEWAY_SESSION_FAILURE");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("/relative/path")]
    public async Task HandleAsync_ShouldThrow_WhenUrlIsInvalid(string url)
    {
        var command = new GetBillingPortalCommand(Guid.NewGuid(), url);
        var act = () => _handler.HandleAsync(command);
        await act.Should().ThrowAsync<BusinessRuleException>().Where(e => e.RuleName == "INVALID_RETURN_URL");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenSchemeIsHttp()
    {
        var command = new GetBillingPortalCommand(Guid.NewGuid(), "http://untrusted.com");
        var act = () => _handler.HandleAsync(command);
        await act.Should().ThrowAsync<BusinessRuleException>().Where(e => e.RuleName == "INVALID_RETURN_URL_SCHEME");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenHostIsNotAllowed()
    {
        var command = new GetBillingPortalCommand(Guid.NewGuid(), "https://evil.com");
        var act = () => _handler.HandleAsync(command);
        await act.Should().ThrowAsync<BusinessRuleException>().Where(e => e.RuleName == "UNTRUSTED_RETURN_HOST");
    }
}
