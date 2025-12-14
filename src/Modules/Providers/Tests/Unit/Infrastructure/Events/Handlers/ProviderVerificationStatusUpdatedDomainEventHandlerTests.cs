using FluentAssertions;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events.Handlers;

public class ProviderVerificationStatusUpdatedDomainEventHandlerTests : IDisposable
{
    private readonly Mock<IMessageBus> _mockMessageBus;
    private readonly Mock<ISearchProvidersModuleApi> _mockSearchProvidersModuleApi;
    private readonly Mock<ILogger<ProviderVerificationStatusUpdatedDomainEventHandler>> _mockLogger;
    private readonly ProvidersDbContext _context;
    private readonly ProviderVerificationStatusUpdatedDomainEventHandler _handler;

    public ProviderVerificationStatusUpdatedDomainEventHandlerTests()
    {
        _mockMessageBus = new Mock<IMessageBus>();
        _mockSearchProvidersModuleApi = new Mock<ISearchProvidersModuleApi>();
        _mockLogger = new Mock<ILogger<ProviderVerificationStatusUpdatedDomainEventHandler>>();

        // Create in-memory database
        var options = new DbContextOptionsBuilder<ProvidersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ProvidersDbContext(options);

        _handler = new ProviderVerificationStatusUpdatedDomainEventHandler(
            _mockMessageBus.Object,
            _context,
            _mockSearchProvidersModuleApi.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = Guid.NewGuid();

        // Add provider to database
        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var businessProfile = new BusinessProfile(
            "Test Provider LTDA",
            new ContactInfo("test@test.com", "1234567890"),
            address);
        var provider = new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        ProviderVerificationStatusUpdatedIntegrationEvent? capturedEvent = null;
        _mockMessageBus
            .Setup(x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<ProviderVerificationStatusUpdatedIntegrationEvent, string, CancellationToken>((evt, _, _) =>
            {
                capturedEvent = evt;
            })
            .Returns(Task.CompletedTask);

        _mockSearchProvidersModuleApi
            .Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value,
            1,
            EVerificationStatus.Pending,
            EVerificationStatus.Verified,
            null
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        capturedEvent.Should().NotBeNull();
        capturedEvent!.ProviderId.Should().Be(providerId.Value);
        capturedEvent.UserId.Should().Be(userId);
        capturedEvent.PreviousStatus.Should().Be(EVerificationStatus.Pending.ToString());
        capturedEvent.NewStatus.Should().Be(EVerificationStatus.Verified.ToString());
        capturedEvent.UpdatedBy.Should().BeNull();
        capturedEvent.Name.Should().Be("Test Provider");

        _mockSearchProvidersModuleApi.Verify(
            x => x.IndexProviderAsync(providerId.Value, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageBusFails_ShouldThrowException()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = Guid.NewGuid();

        // Add provider to database
        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var businessProfile = new BusinessProfile(
            "Test Provider LTDA",
            new ContactInfo("test@test.com", "1234567890"),
            address);
        var provider = new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        _mockSearchProvidersModuleApi
            .Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value,
            1,
            EVerificationStatus.Pending,
            EVerificationStatus.Verified,
            null
        );

        _mockMessageBus
            .Setup(x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus error"));

        // Act
        var act = async () => await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.InnerException.Should().BeOfType<Exception>();
        ex.Which.InnerException!.Message.Should().Be("Message bus error");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling") || v.ToString()!.Contains("Error publishing")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockSearchProvidersModuleApi.Verify(
            x => x.IndexProviderAsync(providerId.Value, It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldLogWarningAndReturn()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId,
            1,
            EVerificationStatus.Pending,
            EVerificationStatus.Verified,
            null
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsRejected_ShouldRemoveFromSearch()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = Guid.NewGuid();

        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var businessProfile = new BusinessProfile(
            "Test Provider LTDA",
            new ContactInfo("test@test.com", "1234567890"),
            address);
        var provider = new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        _mockSearchProvidersModuleApi
            .Setup(x => x.RemoveProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value,
            1,
            EVerificationStatus.Verified,
            EVerificationStatus.Rejected,
            null
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockSearchProvidersModuleApi.Verify(
            x => x.RemoveProviderAsync(providerId.Value, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockSearchProvidersModuleApi.Verify(
            x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsSuspended_ShouldRemoveFromSearch()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = Guid.NewGuid();

        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var businessProfile = new BusinessProfile(
            "Test Provider LTDA",
            new ContactInfo("test@test.com", "1234567890"),
            address);
        var provider = new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        _mockSearchProvidersModuleApi
            .Setup(x => x.RemoveProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value,
            1,
            EVerificationStatus.Verified,
            EVerificationStatus.Suspended,
            null
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockSearchProvidersModuleApi.Verify(
            x => x.RemoveProviderAsync(providerId.Value, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenIndexingFails_ShouldLogErrorAndContinue()
    {
        // Arrange
        var providerId = ProviderId.New();
        var userId = Guid.NewGuid();

        var address = new Address("Rua Teste", "123", "Centro", "São Paulo", "SP", "01234-567", "Brasil");
        var businessProfile = new BusinessProfile(
            "Test Provider LTDA",
            new ContactInfo("test@test.com", "1234567890"),
            address);
        var provider = new Provider(providerId, userId, "Test Provider", EProviderType.Individual, businessProfile);
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        _mockSearchProvidersModuleApi
            .Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure("Failed to index"));

        var domainEvent = new ProviderVerificationStatusUpdatedDomainEvent(
            providerId.Value,
            1,
            EVerificationStatus.Pending,
            EVerificationStatus.Verified,
            null
        );

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to index")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Should still publish integration event
        _mockMessageBus.Verify(
            x => x.PublishAsync(
                It.IsAny<ProviderVerificationStatusUpdatedIntegrationEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
