using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
public class ProviderServicesUpdatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchProvidersModuleApiMock;
    private readonly Mock<ILogger<ProviderServicesUpdatedIntegrationEventHandler>> _loggerMock;
    private readonly ProviderServicesUpdatedIntegrationEventHandler _handler;

    public ProviderServicesUpdatedIntegrationEventHandlerTests()
    {
        _searchProvidersModuleApiMock = new Mock<ISearchProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<ProviderServicesUpdatedIntegrationEventHandler>>();
        _handler = new ProviderServicesUpdatedIntegrationEventHandler(_searchProvidersModuleApiMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenIndexSucceeds_ShouldNotThrow()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderServicesUpdatedIntegrationEvent(
            "Providers", providerId, Array.Empty<Guid>());

        _searchProvidersModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var act = async () => await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _searchProvidersModuleApiMock.Verify(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenIndexFails_ShouldNotThrowException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderServicesUpdatedIntegrationEvent(
            "Providers", providerId, Array.Empty<Guid>());

        _searchProvidersModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(new Error("Index failed", 500)));

        // Act
        var act = async () => await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _searchProvidersModuleApiMock.Verify(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenApiThrowsException_ShouldNotPropagateException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new ProviderServicesUpdatedIntegrationEvent(
            "Providers", providerId, Array.Empty<Guid>());

        _searchProvidersModuleApiMock
            .Setup(x => x.IndexProviderAsync(providerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API error"));

        // Act
        var act = async () => await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
