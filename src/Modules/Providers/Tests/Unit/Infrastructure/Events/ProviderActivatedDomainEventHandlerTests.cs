using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events;

/// <summary>
/// Unit tests for <see cref="ProviderActivatedDomainEventHandler"/>.
/// </summary>
public class ProviderActivatedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly ProviderActivatedDomainEventHandler _handler;

    public ProviderActivatedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        _handler = new ProviderActivatedDomainEventHandler(
            _messageBusMock.Object,
            NullLogger<ProviderActivatedDomainEventHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var domainEvent = new ProviderActivatedDomainEvent(
            UuidGenerator.NewId(),
            1,
            UuidGenerator.NewId(),
            "Provider Test",
            "admin@test.com"
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSystemActivation_ShouldPublishIntegrationEvent()
    {
        // Arrange - System activation (null activatedBy)
        var domainEvent = new ProviderActivatedDomainEvent(
            UuidGenerator.NewId(),
            1,
            UuidGenerator.NewId(),
            "Provider Test",
            null
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCancelled_ShouldPropagateCancellation()
    {
        // Arrange
        var domainEvent = new ProviderActivatedDomainEvent(
            UuidGenerator.NewId(),
            1,
            UuidGenerator.NewId(),
            "Provider Test",
            "admin@test.com"
        );

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Configure mock to throw when called with a cancelled token
        _messageBusMock
            .Setup(m => m.PublishAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns<object, string?, CancellationToken>((evt, topic, token) =>
            {
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.HandleAsync(domainEvent, cts.Token));

        ex.InnerException.Should().BeOfType<OperationCanceledException>();

        // Verify that no successful publish occurred (only attempted)
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.AtMostOnce);
    }
}
