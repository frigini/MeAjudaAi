using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class SubscriptionExpiredIntegrationEventHandlerTests : BaseInMemoryDatabaseTest<ProvidersDbContext>
{
    private readonly Mock<ILogger<SubscriptionExpiredIntegrationEventHandler>> _logger = new();
    private readonly Mock<IIdempotencyRepository> _idempotencyRepositoryMock = new();

    public SubscriptionExpiredIntegrationEventHandlerTests() : base(options => new ProvidersDbContext(options))
    {
    }

    [Fact]
    public async Task HandleAsync_WhenProviderExists_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new ProviderBuilder()
            .WithName("Test Provider")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(businessProfile)
            .Build();
        provider.PromoteTier(EProviderTier.Gold, "setup");
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var evt = new SubscriptionExpiredIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);
        var correlationId = $"{evt.SubscriptionId}_{evt.Id}";
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new SubscriptionExpiredIntegrationEventHandler(DbContext, _idempotencyRepositoryMock.Object, _logger.Object);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEventProcessed_ShouldNotReprocess()
    {
        var evt = new SubscriptionExpiredIntegrationEvent("Payments", Guid.NewGuid(), Guid.NewGuid());
        var correlationId = $"{evt.SubscriptionId}_{evt.Id}";
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new SubscriptionExpiredIntegrationEventHandler(DbContext, _idempotencyRepositoryMock.Object, _logger.Object);

        await handler.HandleAsync(evt);

        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        _idempotencyRepositoryMock.Verify(x => x.MarkAsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Never);
        DbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }
}
