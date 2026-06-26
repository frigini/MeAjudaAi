using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Infrastructure")]
public class ProviderIntegrationEventHandlersTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchApiMock = new();
    private readonly Mock<ILogger<ProviderDeletedIntegrationEventHandler>> _loggerDeletedMock = new();
    private readonly Mock<ILogger<ProviderProfileUpdatedIntegrationEventHandler>> _loggerUpdatedMock = new();

    #region ProviderDeletedIntegrationEventHandler Tests

    [Fact]
    public async Task ProviderDeletedHandler_OnSuccess_ShouldCallRemoveProviderAndLogSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _searchApiMock.Setup(x => x.RemoveProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ProviderDeletedIntegrationEventHandler(_searchApiMock.Object, _loggerDeletedMock.Object);
        var evt = CreateProviderDeletedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchApiMock.Verify(x => x.RemoveProviderAsync(evt.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLog(_loggerDeletedMock, LogLevel.Information, $"Handling ProviderDeletedIntegrationEvent for provider {providerId}", Times.Once());
        VerifyLog(_loggerDeletedMock, LogLevel.Information, $"Provider {providerId} removed from search index successfully after deletion", Times.Once());
    }

    [Fact]
    public async Task ProviderDeletedHandler_OnFailure_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var errorMessage = "Index error";
        _searchApiMock.Setup(x => x.RemoveProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(errorMessage, Code: "Search.Error")));

        var handler = new ProviderDeletedIntegrationEventHandler(_searchApiMock.Object, _loggerDeletedMock.Object);
        var evt = CreateProviderDeletedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchApiMock.Verify(x => x.RemoveProviderAsync(evt.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLog(_loggerDeletedMock, LogLevel.Error, $"Failed to remove provider {providerId} from search index after deletion: {errorMessage}", Times.Once());
    }

    [Fact]
    public async Task ProviderDeletedHandler_OnException_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        _searchApiMock.Setup(x => x.RemoveProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new ProviderDeletedIntegrationEventHandler(_searchApiMock.Object, _loggerDeletedMock.Object);
        var evt = CreateProviderDeletedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        VerifyLog(_loggerDeletedMock, LogLevel.Error, $"Error handling ProviderDeletedIntegrationEvent for provider {providerId}", Times.Once());
    }

    #endregion

    #region ProviderProfileUpdatedIntegrationEventHandler Tests

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnSuccess_ShouldCallIndexProviderAndLogSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerUpdatedMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchApiMock.Verify(x => x.IndexProviderAsync(evt.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLog(_loggerUpdatedMock, LogLevel.Information, $"Handling ProviderProfileUpdatedIntegrationEvent for provider {providerId}", Times.Once());
        VerifyLog(_loggerUpdatedMock, LogLevel.Information, $"Provider {providerId} reindexed successfully after profile update", Times.Once());
    }

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnFailure_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var errorMessage = "Index error";
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error(errorMessage, Code: "Search.Error")));

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerUpdatedMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchApiMock.Verify(x => x.IndexProviderAsync(evt.ProviderId, It.IsAny<CancellationToken>()), Times.Once);
        VerifyLog(_loggerUpdatedMock, LogLevel.Error, $"Failed to reindex provider {providerId} after profile update: {errorMessage}", Times.Once());
    }

    [Fact]
    public async Task ProviderProfileUpdatedHandler_OnException_ShouldLogError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var exception = new Exception("Unexpected error");
        _searchApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var handler = new ProviderProfileUpdatedIntegrationEventHandler(_searchApiMock.Object, _loggerUpdatedMock.Object);
        var evt = CreateProviderProfileUpdatedEvent(providerId);

        // Act
        await handler.HandleAsync(evt);

        // Assert
        VerifyLog(_loggerUpdatedMock, LogLevel.Error, $"Error handling ProviderProfileUpdatedIntegrationEvent for provider {providerId}", Times.Once());
    }

    #endregion

    #region Helpers

    private static ProviderDeletedIntegrationEvent CreateProviderDeletedEvent(Guid providerId)
        => new(
            Source: "Providers",
            ProviderId: providerId,
            UserId: Guid.NewGuid(),
            Email: "test@provider.com",
            Name: "Test Provider",
            Reason: "Deleted",
            DeletedAt: DateTime.UtcNow,
            DeletedBy: null);

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

    #endregion
}
