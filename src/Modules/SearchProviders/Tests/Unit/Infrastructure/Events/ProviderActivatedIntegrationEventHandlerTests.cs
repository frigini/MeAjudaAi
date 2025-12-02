using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events;

public class ProviderActivatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchModuleApiMock;
    private readonly Mock<ILogger<ProviderActivatedIntegrationEventHandler>> _loggerMock;
    private readonly ProviderActivatedIntegrationEventHandler _handler;

    public ProviderActivatedIntegrationEventHandlerTests()
    {
        _searchModuleApiMock = new Mock<ISearchProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<ProviderActivatedIntegrationEventHandler>>();
        _handler = new ProviderActivatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidProvider_ShouldCallIndexProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var integrationEvent = new ProviderActivatedIntegrationEvent(
            "Providers",
            providerId,
            userId,
            "Dr. João Silva"
        );

        _searchModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        _searchModuleApiMock.Verify(
            x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenIndexingFails_ShouldLogErrorButNotThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var integrationEvent = new ProviderActivatedIntegrationEvent(
            "Providers",
            providerId,
            userId,
            "Dr. João Silva"
        );

        _searchModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.Internal("Search service unavailable")));

        // Act (should not throw)
        await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        _searchModuleApiMock.Verify(
            x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to index")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenExceptionThrown_ShouldLogErrorButNotThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var integrationEvent = new ProviderActivatedIntegrationEvent(
            "Providers",
            providerId,
            userId,
            "Dr. João Silva"
        );

        _searchModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act (should not throw)
        await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        _searchModuleApiMock.Verify(
            x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WithMultipleProviders_ShouldIndexEach()
    {
        // Arrange
        var providerData = new[]
        {
            (Guid.NewGuid(), Guid.NewGuid(), "Dr. João Silva"),
            (Guid.NewGuid(), Guid.NewGuid(), "Dra. Maria Santos"),
            (Guid.NewGuid(), Guid.NewGuid(), "Dr. Pedro Costa")
        };

        _searchModuleApiMock
            .Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        foreach (var (providerId, userId, name) in providerData)
        {
            var integrationEvent = new ProviderActivatedIntegrationEvent(
                "Providers",
                providerId,
                userId,
                name
            );

            // Act
            await _handler.HandleAsync(integrationEvent, CancellationToken.None);
        }

        // Assert
        _searchModuleApiMock.Verify(
            x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Exactly(providerData.Length)
        );
    }
}
