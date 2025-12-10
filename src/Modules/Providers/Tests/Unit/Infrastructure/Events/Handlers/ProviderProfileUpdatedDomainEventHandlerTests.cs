using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

/// <summary>
/// Unit tests for <see cref="ProviderProfileUpdatedDomainEventHandler"/> testing the handler's behavior
/// when processing provider profile update events and publishing integration events.
/// </summary>
public class ProviderProfileUpdatedDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderProfileUpdatedDomainEventHandler>> _mockLogger;
    private readonly ProvidersDbContext _context;
    private readonly ProviderProfileUpdatedDomainEventHandler _handler;

    public ProviderProfileUpdatedDomainEventHandlerTests()
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderProfileUpdatedDomainEventHandler>>();

        // Create in-memory database
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: UuidGenerator.NewId().ToString())
            .Options;
        _context = new ProvidersDbContext(options);

        _handler = new ProviderProfileUpdatedDomainEventHandler(_mockMessageBus.Object, _context, _mockLogger.Object);
    }

    /// <summary>
    /// Verifies that HandleAsync publishes a ProviderProfileUpdatedIntegrationEvent with correct provider details
    /// when processing a valid profile update domain event.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = UuidGenerator.NewId();

        var provider = CreateTestProvider(providerId, userId);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderProfileUpdatedDomainEvent(
            providerId.Value,
            1,
            "Updated Name",
            "updated@test.com",
            null,
            new[] { "Name", "Email" }
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.Is<ProviderProfileUpdatedIntegrationEvent>(e =>
                    e.ProviderId == providerId.Value &&
                    e.Name == "Updated Name"),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that when message bus publishing fails, the handler logs an error containing
    /// "Error handling ProviderProfileUpdatedDomainEvent" and re-throws the exception.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldThrowException()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = UuidGenerator.NewId();

        var provider = CreateTestProvider(providerId, userId);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderProfileUpdatedDomainEvent(
            providerId.Value,
            1,
            "Updated Name",
            "updated@test.com",
            null,
            new[] { "Name", "Email" }
        );

        _mockMessageBus
            .Setup(x => x.PublishAsync(
                It.IsAny<ProviderProfileUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Message bus error");

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
        return new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
