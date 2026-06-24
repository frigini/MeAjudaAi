using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

public class ProviderVerificationStatusUpdatedDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderVerificationStatusUpdatedDomainEventHandler>> _mockLogger;
    private readonly ProvidersDbContext _context;
    private readonly ProviderVerificationStatusUpdatedDomainEventHandler _handler;

    public ProviderVerificationStatusUpdatedDomainEventHandlerTests()
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderVerificationStatusUpdatedDomainEventHandler>>();

        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ProvidersDbContext(options, null!);

        _handler = new ProviderVerificationStatusUpdatedDomainEventHandler(
            _mockMessageBus.Object,
            _context,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvents()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = Guid.NewGuid();

        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithUserId(userId)
            .WithName("Test Provider")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(new BusinessProfile("Test", new ContactInfo("t@t.com", "123"), new Address("R", "1", "C", "S", "S", "1", "B")))
            .Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var publishedEvents = new List<object>();
        _mockMessageBus
            .Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<object, string, CancellationToken>((evt, _, _) => publishedEvents.Add(evt))
            .Returns(Task.CompletedTask);

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value, 1, EVerificationStatus.Pending, EVerificationStatus.Verified, null);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        publishedEvents.Should().HaveCount(2);
        publishedEvents.Should().ContainSingle(e => e is ProviderIndexRequiredIntegrationEvent);
        publishedEvents.Should().ContainSingle(e => e is ProviderVerificationStatusUpdatedIntegrationEvent);
        
        publishedEvents.OfType<ProviderIndexRequiredIntegrationEvent>().Single().ProviderId.Should().Be(providerId.Value);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldThrowException()
    {
        // Arrange
        var providerId = ProviderId.New();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithName("Test")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(new BusinessProfile("T", new ContactInfo("t@t.com", "1"), new Address("R", "1", "C", "S", "S", "1", "B")))
            .Build();
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        _mockMessageBus
            .Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value, 1, EVerificationStatus.Pending, EVerificationStatus.Verified, null);

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Message bus error");
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldLogWarningAndReturn()
    {
        // Arrange
        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(Guid.NewGuid(), 1, EVerificationStatus.Pending, EVerificationStatus.Verified, null);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        _mockMessageBus.Verify(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
