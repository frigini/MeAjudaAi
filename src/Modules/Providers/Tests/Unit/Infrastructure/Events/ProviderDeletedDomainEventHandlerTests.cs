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

public class ProviderDeletedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly ProvidersDbContext _context;
    private readonly ProviderDeletedDomainEventHandler _handler;

    public ProviderDeletedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new ProvidersDbContext(options);
        
        _handler = new ProviderDeletedDomainEventHandler(
            _messageBusMock.Object,
            _context,
            NullLogger<ProviderDeletedDomainEventHandler>.Instance);
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

        var domainEvent = new ProviderDeletedDomainEvent(
            providerId.Value,
            1,
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
    public async Task HandleAsync_WhenProviderNotFound_ShouldNotPublishIntegrationEvent()
    {
        // Arrange
        var domainEvent = new ProviderDeletedDomainEvent(
            Guid.NewGuid(),
            1,
            "Nonexistent Provider",
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
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithSystemDeletion_ShouldPublishIntegrationEvent()
    {
        // Arrange - System deletion (null deletedBy)
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

        var domainEvent = new ProviderDeletedDomainEvent(
            providerId.Value,
            1,
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
}
