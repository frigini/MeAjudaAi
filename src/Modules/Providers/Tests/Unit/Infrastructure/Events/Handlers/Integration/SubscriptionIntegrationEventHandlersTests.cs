using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class SubscriptionIntegrationEventHandlersTests : BaseInMemoryDatabaseTest<ProvidersDbContext>
{
    private readonly Mock<ILogger<SubscriptionActivatedIntegrationEventHandler>> _loggerActivated = new();
    private readonly Mock<ILogger<SubscriptionCanceledIntegrationEventHandler>> _loggerCanceled = new();
    private readonly Mock<ILogger<SubscriptionExpiredIntegrationEventHandler>> _loggerExpired = new();
    private readonly Mock<IIdempotencyRepository> _idempotencyRepositoryMock = new();

    public SubscriptionIntegrationEventHandlersTests() : base(options => new ProvidersDbContext(options))
    {
    }

    [Fact]
    public async Task ActivatedHandler_WhenProviderExists_ShouldPromoteToGold()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var evt = new SubscriptionActivatedIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);
        var correlationId = evt.SubscriptionId.ToString();
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SubscriptionActivatedIntegrationEventHandler(DbContext, _idempotencyRepositoryMock.Object, _loggerActivated.Object);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Gold);
        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivatedHandler_WhenEventProcessed_ShouldNotReprocess()
    {
        var correlationId = Guid.NewGuid().ToString();
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new SubscriptionActivatedIntegrationEventHandler(DbContext, _idempotencyRepositoryMock.Object, _loggerActivated.Object);
        var evt = new SubscriptionActivatedIntegrationEvent("Payments", Guid.Parse(correlationId), Guid.NewGuid());

        await handler.HandleAsync(evt);

        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        DbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task CanceledHandler_WhenProviderExists_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        provider.PromoteTier(EProviderTier.Gold, "setup");
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var evt = new SubscriptionCanceledIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);
        var correlationId = evt.SubscriptionId.ToString();
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SubscriptionCanceledIntegrationEventHandler(DbContext, _idempotencyRepositoryMock.Object, _loggerCanceled.Object);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExpiredHandler_WhenProviderExists_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        provider.PromoteTier(EProviderTier.Gold, "setup");
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var evt = new SubscriptionExpiredIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);
        var correlationId = evt.SubscriptionId.ToString();
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SubscriptionExpiredIntegrationEventHandler(DbContext, _idempotencyRepositoryMock.Object, _loggerExpired.Object);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
