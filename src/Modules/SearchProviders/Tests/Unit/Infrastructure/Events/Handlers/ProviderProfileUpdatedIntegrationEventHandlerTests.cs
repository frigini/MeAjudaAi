using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Infrastructure")]
public class ProviderProfileUpdatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchApiMock = new();
    private readonly Mock<ILogger<ProviderProfileUpdatedIntegrationEventHandler>> _loggerMock = new();

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnSuccess_ShouldCallIndexProviderAndLogSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchApiMock.Verify(x => x.IndexProviderAsync(evt.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLog(_loggerMock, LogLevel.Information, $"Handling ProviderProfileUpdatedIntegrationEvent for provider {providerId}", Times.Once());
        VerifyLog(_loggerMock, LogLevel.Information, $"Provider {providerId} reindexed successfully after profile update", Times.Once());
    }

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnFailure_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var errorMessage = "Index error";
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(errorMessage, Code: "Search.Error")));

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchApiMock.Verify(x => x.IndexProviderAsync(evt.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLog(_loggerMock, LogLevel.Error, $"Failed to reindex provider {providerId} after profile update: {errorMessage}", Times.Once());
    }

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnException_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        VerifyLog(_loggerMock, LogLevel.Error, $"Error handling ProviderProfileUpdatedIntegrationEvent for provider {providerId}", Times.Once());
    }

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnCancellation_ShouldRethrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act & Assert
        await handler.Invoking(h => h.HandleAsync(evt, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnHttpRequestException_ShouldLogHttpError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        VerifyLog(_loggerMock, LogLevel.Error, $"HTTP error handling ProviderProfileUpdatedIntegrationEvent for provider {providerId}", Times.Once());
    }

    private static ProviderProfileUpdatedIntegrationEvent CreateProviderProfileUpdatedEvent(Guid providerId)
        => new(
            Source: "Providers",
            ProviderId: providerId,
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            UpdatedFields: new List<string> { "Bio", "ProfileImage" });

    private void VerifyLog<T>(Mock<ILogger<T>> loggerMock, LogLevel level, string messagePart, Times times)
    {
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == level),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messagePart)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
