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
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

public class ProviderProfileUpdatedDomainEventHandlerTests : BaseInMemoryDatabaseTest<ProvidersDbContext>
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderProfileUpdatedDomainEventHandler>> _mockLogger;

    public ProviderProfileUpdatedDomainEventHandlerTests() : base(options => new ProvidersDbContext(options, null!))
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderProfileUpdatedDomainEventHandler>>();
    }

    private ProviderProfileUpdatedDomainEventHandler CreateHandler() =>
        new(_mockMessageBus.Object, DbContext, _mockLogger.Object);

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        var providerId = ProviderId.New();
        var userId = UuidGenerator.NewId();

        var provider = CreateTestProvider(providerId, userId);
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var domainEvent = new ProviderProfileUpdatedDomainEvent(
            providerId.Value,
            1,
            "Updated Name",
            "updated@test.com",
            "updated-name",
            null,
            new[] { "Name", "Email" }
        );

        await CreateHandler().HandleAsync(domainEvent, CancellationToken.None);

        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.Is<ProviderProfileUpdatedIntegrationEvent>(e =>
                    e.ProviderId == providerId.Value &&
                    e.Name == "Updated Name" &&
                    e.Slug == "updated-name"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldThrowException()
    {
        var providerId = ProviderId.New();
        var userId = UuidGenerator.NewId();

        var provider = CreateTestProvider(providerId, userId);
        DbContext.Providers.Add(provider);
        await DbContext.SaveChangesAsync();

        var domainEvent = new ProviderProfileUpdatedDomainEvent(
            providerId.Value,
            1,
            "Updated Name",
            "updated@test.com",
            "updated-name",
            null,
            new[] { "Name", "Email" }
        );

        _mockMessageBus
            .Setup(x => x.PublishAsync(
                It.IsAny<ProviderProfileUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        var act = async () => await CreateHandler().HandleAsync(domainEvent, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.InnerException.Should().BeOfType<Exception>();
        ex.Which.InnerException!.Message.Should().Be("Message bus error");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling ProviderProfileUpdatedDomainEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static Provider CreateTestProvider(ProviderId providerId, Guid userId)
    {
        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var businessProfile = new BusinessProfile(
            "Test Provider LTDA",
            new ContactInfo("test@test.com", "1234567890"),
            address);
        return new ProviderBuilder()
            .WithId(providerId)
            .WithUserId(userId)
            .WithName("Test Provider")
            .WithType(EProviderType.Individual)
            .WithBusinessProfile(businessProfile)
            .Build();
    }
}
