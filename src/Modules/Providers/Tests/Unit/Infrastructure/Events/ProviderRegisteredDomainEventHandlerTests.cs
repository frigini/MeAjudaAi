using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProviderEntity = MeAjudaAi.Modules.Providers.Domain.Entities.Provider;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events;

public class ProviderRegisteredDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly ProvidersDbContext _context;
    private readonly ProviderRegisteredDomainEventHandler _handler;

    public ProviderRegisteredDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new ProvidersDbContext(options);
        
        _handler = new ProviderRegisteredDomainEventHandler(
            _messageBusMock.Object,
            _context,
            NullLogger<ProviderRegisteredDomainEventHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = new ProviderId(Guid.NewGuid());
        var userId = Guid.NewGuid();
        var provider = ProviderEntity.Create(
            providerId,
            userId,
            "Provider Test",
            EProviderType.Individual,
            "test@provider.com"
        );
        
        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderRegisteredDomainEvent(
            providerId.Value,
            1,
            userId,
            "Provider Test",
            EProviderType.Individual,
            "test@provider.com"
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
    public async Task HandleAsync_WhenProviderNotFound_ShouldNotPublishIntegrationEvent()
    {
        // Arrange
        var domainEvent = new ProviderRegisteredDomainEvent(
            Guid.NewGuid(),
            1,
            Guid.NewGuid(),
            "Nonexistent Provider",
            EProviderType.Individual,
            "test@provider.com"
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCancelled_ShouldRespectCancellation()
    {
        // Arrange
        var providerId = new ProviderId(Guid.NewGuid());
        var domainEvent = new ProviderRegisteredDomainEvent(
            providerId.Value,
            1,
            Guid.NewGuid(),
            "Provider Test",
            EProviderType.Individual,
            "test@provider.com"
        );
        
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
