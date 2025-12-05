using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Infrastructure.Events;

public class ProviderActivatedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderActivatedDomainEventHandler>> _mockLogger;
    private readonly ProviderActivatedDomainEventHandler _handler;

    public ProviderActivatedDomainEventHandlerTests()
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderActivatedDomainEventHandler>>();
        _handler = new ProviderActivatedDomainEventHandler(_mockMessageBus.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.Is<ProviderActivatedIntegrationEvent>(e => e.ProviderId == providerId),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformation_WhenHandling()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling ProviderActivatedDomainEvent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogSuccess_AfterPublishing()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully published")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId);
        using var cts = new CancellationTokenSource();

        // Act
        await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderActivatedIntegrationEvent>(),
                It.IsAny<string>(),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogError_WhenPublishFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId);
        var exception = new InvalidOperationException("Publish failed");

        _mockMessageBus.Setup(x => x.PublishAsync(
                It.IsAny<ProviderActivatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapDomainEventToIntegrationEvent_Correctly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderActivatedDomainEvent(providerId);
        ProviderActivatedIntegrationEvent? capturedEvent = null;

        _mockMessageBus.Setup(x => x.PublishAsync(
                It.IsAny<ProviderActivatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<ProviderActivatedIntegrationEvent, string, CancellationToken>((e, t, ct) => capturedEvent = e)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.ProviderId.Should().Be(providerId);
        capturedEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
