using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events;

public class ProviderRegisteredDomainEventHandlerTests : BaseInMemoryDatabaseTest<ProvidersDbContext>
{
    private readonly Mock<IMessageBus> _messageBusMock;

    public ProviderRegisteredDomainEventHandlerTests() : base(options => new ProvidersDbContext(options, null!))
    {
        _messageBusMock = new Mock<IMessageBus>();
    }

    private ProviderRegisteredDomainEventHandler CreateHandler() =>
        new(_messageBusMock.Object, DbContext, NullLogger<ProviderRegisteredDomainEventHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldPublishIntegrationEvent()
    {
        var providerId = new ProviderId(UuidGenerator.NewId());
        var userId = UuidGenerator.NewId();

        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithUserId(userId)
            .WithName("Provider Test")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(new MeAjudaAi.Modules.Providers.Domain.ValueObjects.BusinessProfile(
                "Test Company",
                new MeAjudaAi.Modules.Providers.Domain.ValueObjects.ContactInfo("test@provider.com", "+55 11 99999-9999", "https://www.test.com"),
                new MeAjudaAi.Modules.Providers.Domain.ValueObjects.Address("Test St", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil")))
            .Build();

        await DbContext.Providers.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var domainEvent = new ProviderRegisteredDomainEvent(
            providerId.Value,
            1,
            userId,
            "Provider Test",
            EProviderType.Individual,
            "test@provider.com",
            "provider-test"
        );

        await CreateHandler().HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<ProviderRegisteredIntegrationEvent>(e =>
                    e.Slug == "provider-test" &&
                    e.Name == "Provider Test"),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMissingProvider_ShouldNotPublishEvent()
    {
        var domainEvent = new ProviderRegisteredDomainEvent(
            UuidGenerator.NewId(),
            1,
            UuidGenerator.NewId(),
            "Nonexistent Provider",
            EProviderType.Individual,
            "test@provider.com",
            "nonexistent-provider"
        );

        await CreateHandler().HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCancelled_ShouldPropagateCancellation()
    {
        var providerId = new ProviderId(UuidGenerator.NewId());
        var domainEvent = new ProviderRegisteredDomainEvent(
            providerId.Value,
            1,
            UuidGenerator.NewId(),
            "Provider Test",
            EProviderType.Individual,
            "test@provider.com",
            "provider-test"
        );

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await CreateHandler().HandleAsync(domainEvent, cts.Token);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.InnerException.Should().BeOfType<OperationCanceledException>();

        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
