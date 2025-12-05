using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Infrastructure.Events;

public class ProviderRegisteredDomainEventHandlerTests : IDisposable
{
    private readonly ProvidersDbContext _context;
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ILogger<ProviderRegisteredDomainEventHandler>> _mockLogger;
    private readonly ProviderRegisteredDomainEventHandler _handler;

    public ProviderRegisteredDomainEventHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new ProvidersDbContext(options);
        _mockMessageBus = new Mock<IMessageBus>();
        _mockLogger = new Mock<ILogger<ProviderRegisteredDomainEventHandler>>();
        _handler = new ProviderRegisteredDomainEventHandler(_mockMessageBus.Object, _context, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent_WhenProviderExists()
    {
        // Arrange
        var providerId = ProviderId.CreateUnique();
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "João Silva",
            "12345678900",
            "joao@email.com",
            "11987654321");

        // Use reflection to set the Id
        var idProperty = typeof(Provider).GetProperty("Id");
        idProperty!.SetValue(provider, providerId);

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderRegisteredDomainEvent(providerId.Value, userId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.Is<ProviderRegisteredIntegrationEvent>(e => e.ProviderId == providerId.Value),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogWarning_WhenProviderNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domainEvent = new ProviderRegisteredDomainEvent(providerId, userId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderRegisteredIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogInformation_WhenHandling()
    {
        // Arrange
        var providerId = ProviderId.CreateUnique();
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "João Silva",
            "12345678900",
            "joao@email.com",
            "11987654321");

        var idProperty = typeof(Provider).GetProperty("Id");
        idProperty!.SetValue(provider, providerId);

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderRegisteredDomainEvent(providerId.Value, userId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling ProviderRegisteredDomainEvent")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var providerId = ProviderId.CreateUnique();
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "João Silva",
            "12345678900",
            "joao@email.com",
            "11987654321");

        var idProperty = typeof(Provider).GetProperty("Id");
        idProperty!.SetValue(provider, providerId);

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderRegisteredDomainEvent(providerId.Value, userId);
        using var cts = new CancellationTokenSource();

        // Act
        await _handler.HandleAsync(domainEvent, cts.Token);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderRegisteredIntegrationEvent>(),
                It.IsAny<string>(),
                cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldLogSuccess_AfterPublishing()
    {
        // Arrange
        var providerId = ProviderId.CreateUnique();
        var userId = Guid.NewGuid();
        var provider = Provider.Create(
            userId,
            "João Silva",
            "12345678900",
            "joao@email.com",
            "11987654321");

        var idProperty = typeof(Provider).GetProperty("Id");
        idProperty!.SetValue(provider, providerId);

        await _context.Providers.AddAsync(provider);
        await _context.SaveChangesAsync();

        var domainEvent = new ProviderRegisteredDomainEvent(providerId.Value, userId);

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully published")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
