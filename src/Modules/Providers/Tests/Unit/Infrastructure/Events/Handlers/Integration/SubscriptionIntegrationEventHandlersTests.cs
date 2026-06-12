using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers.Integration;

public class SubscriptionIntegrationEventHandlersTests
{
    private readonly ProvidersDbContext _dbContext;
    private readonly Mock<ILogger<SubscriptionActivatedIntegrationEventHandler>> _loggerActivated = new();
    private readonly Mock<ILogger<SubscriptionCanceledIntegrationEventHandler>> _loggerCanceled = new();
    private readonly Mock<ILogger<SubscriptionExpiredIntegrationEventHandler>> _loggerExpired = new();
    private readonly Mock<IIdempotencyRepository> _idempotencyRepositoryMock = new();

    public SubscriptionIntegrationEventHandlersTests()
    {
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProvidersDbContext(options);
    }

    [Fact]
    public async Task ActivatedHandler_WhenProviderExists_ShouldPromoteToGold()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var handler = new SubscriptionActivatedIntegrationEventHandler(_dbContext, _idempotencyRepositoryMock.Object, _loggerActivated.Object);
        var evt = new SubscriptionActivatedIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Gold);
    }

    [Fact]
    public async Task ActivatedHandler_WhenEventProcessed_ShouldNotReprocess()
    {
        var correlationId = Guid.NewGuid().ToString();
        _idempotencyRepositoryMock.Setup(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new SubscriptionActivatedIntegrationEventHandler(_dbContext, _idempotencyRepositoryMock.Object, _loggerActivated.Object);
        var evt = new SubscriptionActivatedIntegrationEvent("Payments", Guid.Parse(correlationId), Guid.NewGuid());

        await handler.HandleAsync(evt);

        _idempotencyRepositoryMock.Verify(x => x.IsProcessedAsync(correlationId, It.IsAny<CancellationToken>()), Times.Once);
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task CanceledHandler_WhenProviderExists_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        provider.PromoteTier(EProviderTier.Gold, "setup");
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var handler = new SubscriptionCanceledIntegrationEventHandler(_dbContext, _idempotencyRepositoryMock.Object, _loggerCanceled.Object);
        var evt = new SubscriptionCanceledIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
    }

    [Fact]
    public async Task ExpiredHandler_WhenProviderExists_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        provider.PromoteTier(EProviderTier.Gold, "setup");
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var handler = new SubscriptionExpiredIntegrationEventHandler(_dbContext, _idempotencyRepositoryMock.Object, _loggerExpired.Object);
        var evt = new SubscriptionExpiredIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
    }
}
