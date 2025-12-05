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
/// Unit tests for <see cref="ProviderRegisteredDomainEventHandler"/>.
/// </summary>
public class ProviderRegisteredDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _messageBusMock;
    private readonly ProvidersDbContext _context;
    private readonly ProviderRegisteredDomainEventHandler _handler;

    public ProviderRegisteredDomainEventHandlerTests()
    {
        _messageBusMock = new Mock<IMessageBus>();
        
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{UuidGenerator.NewId()}")
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
    public async Task HandleAsync_WithMissingProvider_ShouldNotPublishEvent()
    {
        // Arrange
        var domainEvent = new ProviderRegisteredDomainEvent(
            UuidGenerator.NewId(),
            1,
            UuidGenerator.NewId(),
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
        var providerId = new ProviderId(UuidGenerator.NewId());
        var domainEvent = new ProviderRegisteredDomainEvent(
            providerId.Value,
            1,
            UuidGenerator.NewId(),
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

    public void Dispose()
    {
        _context?.Dispose();
    }
}
