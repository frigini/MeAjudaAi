using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

public class ProviderIndexRequiredIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchProvidersModuleApiMock;
    private readonly Mock<ILogger<ProviderIndexRequiredIntegrationEventHandler>> _loggerMock;
    private readonly ProviderIndexRequiredIntegrationEventHandler _handler;

    public ProviderIndexRequiredIntegrationEventHandlerTests()
    {
        _searchProvidersModuleApiMock = new Mock<ISearchProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<ProviderIndexRequiredIntegrationEventHandler>>();
        _handler = new ProviderIndexRequiredIntegrationEventHandler(
            _searchProvidersModuleApiMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallApiAndLogSuccess_WhenIndexingSucceeds()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderIndexRequiredIntegrationEvent("Test", providerId);

        _searchProvidersModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _searchProvidersModuleApiMock.Verify(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verifies success log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("indexed in SearchProviders successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallApiAndLogError_WhenIndexingFails()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderIndexRequiredIntegrationEvent("Test", providerId);
        var expectedError = Error.BadRequest("Failed to index", "INDEX_ERROR");

        _searchProvidersModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(expectedError));

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _searchProvidersModuleApiMock.Verify(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
        
        // Verifies error log
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to index provider")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
