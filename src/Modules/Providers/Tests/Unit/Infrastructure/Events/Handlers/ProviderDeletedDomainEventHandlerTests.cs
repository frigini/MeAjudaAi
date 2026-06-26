using MeAjudaAi.Modules.Providers.Domain.Entities;
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

public class ProviderDeletedDomainEventHandlerTests : BaseInMemoryDatabaseTest<ProvidersDbContext>
{
    private readonly Mock<IMessageBus> _messageBusMock;

    public ProviderDeletedDomainEventHandlerTests() : base(options => new ProvidersDbContext(options, null!))
    {
        _messageBusMock = new Mock<IMessageBus>();
    }

    private ProviderDeletedDomainEventHandler CreateHandler() =>
        new(_messageBusMock.Object, DbContext, NullLogger<ProviderDeletedDomainEventHandler>.Instance);

    [Fact]
    public async Task HandleAsync_WithValidEvent_ShouldPublishIntegrationEvent()
    {
        var providerId = new ProviderId(UuidGenerator.NewId());
        var userId = UuidGenerator.NewId();

        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithUserId(userId)
            .WithName("Provider Test")
            .WithType(MeAjudaAi.Modules.Providers.Domain.Enums.EProviderType.Individual)
            .WithBusinessProfile(new MeAjudaAi.Modules.Providers.Domain.ValueObjects.BusinessProfile(
                "Test Company",
                new MeAjudaAi.Modules.Providers.Domain.ValueObjects.ContactInfo("test@provider.com", "+55 11 99999-9999", "https://www.test.com"),
                new MeAjudaAi.Modules.Providers.Domain.ValueObjects.Address("Test St", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil")))
            .Build();

        await DbContext.Providers.AddAsync(provider);
        await DbContext.SaveChangesAsync();

        var domainEvent = new ProviderDeletedDomainEvent(
            providerId.Value,
            1,
            "Provider Test",
            "admin@test.com"
        );

        await CreateHandler().HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.Is<ProviderDeletedIntegrationEvent>(e =>
                    e.ProviderId == providerId.Value &&
                    e.Name == "Provider Test" &&
                    e.Email == "test@provider.com" &&
                    e.DeletedBy == "admin@test.com"),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldNotPublishIntegrationEvent()
    {
        var domainEvent = new ProviderDeletedDomainEvent(
            UuidGenerator.NewId(),
            1,
            "Nonexistent Provider",
            "admin@test.com"
        );

        await CreateHandler().HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(
            x => x.PublishAsync(
                It.IsAny<object>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
