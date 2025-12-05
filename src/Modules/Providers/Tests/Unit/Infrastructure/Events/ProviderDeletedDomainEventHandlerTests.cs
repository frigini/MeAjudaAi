using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events;

/// <summary>
/// Unit tests for <see cref="ProviderDeletedDomainEventHandler"/>.
/// </summary>
public class ProviderDeletedDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly ProvidersDbContext _context;
    private readonly ProviderDeletedDomainEventHandler _handler;

    public ProviderDeletedDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{UuidGenerator.NewId()}")
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
        var providerId = new ProviderId(UuidGenerator.NewId());
        var userId = UuidGenerator.NewId();
        
        var businessProfile = new BusinessProfile(
            legalName: "Test Company",
            contactInfo: new ContactInfo("test@provider.com", "+55 11 99999-9999", "https://www.test.com"),
            primaryAddress: new Address("Test St", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil"));
        
        var provider = new MeAjudaAi.Modules.Providers.Domain.Entities.Provider(
            providerId,
            userId,
            "Provider Test",
            EProviderType.Individual,
            businessProfile);
        
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
                It.Is<object>(e => e.GetType().Name == "ProviderDeletedIntegrationEvent"),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldNotPublishIntegrationEvent()
    {
        // Arrange
        var domainEvent = new ProviderDeletedDomainEvent(
            UuidGenerator.NewId(),
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
        var providerId = new ProviderId(UuidGenerator.NewId());
        var userId = UuidGenerator.NewId();
        
        var businessProfile = new BusinessProfile(
            legalName: "Test Company",
            contactInfo: new ContactInfo("test@provider.com", "+55 11 99999-9999", null),
            primaryAddress: new Address("Test St", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil"));
        
        var provider = new MeAjudaAi.Modules.Providers.Domain.Entities.Provider(
            providerId,
            userId,
            "Provider Test",
            EProviderType.Individual,
            businessProfile);
        
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

        // Assert - Integration event should still be published (system deletion)
        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<object>(e => e.GetType().Name == "ProviderDeletedIntegrationEvent"),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    public void Dispose() => _context.Dispose();
}
