using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
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

        var handler = new SubscriptionActivatedIntegrationEventHandler(_dbContext, _loggerActivated.Object);
        var evt = new SubscriptionActivatedIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Gold);
    }

    [Fact]
    public async Task ActivatedHandler_WhenEventProcessed_ShouldNotReprocess()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var correlationId = Guid.NewGuid().ToString();
        _dbContext.ProcessedIntegrationEvents.Add(new MeAjudaAi.Modules.Providers.Domain.Entities.ProcessedIntegrationEvent(correlationId, DateTime.UtcNow));
        await _dbContext.SaveChangesAsync();

        var handler = new SubscriptionActivatedIntegrationEventHandler(_dbContext, _loggerActivated.Object);
        var evt = new SubscriptionActivatedIntegrationEvent("Payments", Guid.Parse(correlationId), provider.UserId);

        await handler.HandleAsync(evt);

        var refreshedProvider = await _dbContext.Providers.FindAsync(provider.Id);
        refreshedProvider!.Tier.Should().Be(EProviderTier.Standard);
        _dbContext.ProcessedIntegrationEvents.Count().Should().Be(1);
        _dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task CanceledHandler_WhenProviderIsGold_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        provider.PromoteTier(EProviderTier.Gold, "setup");
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var handler = new SubscriptionCanceledIntegrationEventHandler(_dbContext, _loggerCanceled.Object);
        var evt = new SubscriptionCanceledIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
    }

    [Fact]
    public async Task ExpiredHandler_WhenProviderIsGold_ShouldDemoteToStandard()
    {
        var contactInfo = new ContactInfo("test@test.com");
        var businessProfile = new BusinessProfile("Test Provider", contactInfo, null);
        var provider = new Provider(Guid.NewGuid(), "Test Provider", EProviderType.Individual, businessProfile);
        provider.PromoteTier(EProviderTier.Gold, "setup");
        _dbContext.Providers.Add(provider);
        await _dbContext.SaveChangesAsync();

        var handler = new SubscriptionExpiredIntegrationEventHandler(_dbContext, _loggerExpired.Object);
        var evt = new SubscriptionExpiredIntegrationEvent("Payments", Guid.NewGuid(), provider.UserId);

        await handler.HandleAsync(evt);

        provider.Tier.Should().Be(EProviderTier.Standard);
    }
}
