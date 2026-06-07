using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

public class ServiceActivatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchProvidersModuleApiMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly Mock<ILogger<ServiceActivatedIntegrationEventHandler>> _loggerMock;
    private readonly ServiceActivatedIntegrationEventHandler _handler;

    public ServiceActivatedIntegrationEventHandlerTests()
    {
        _searchProvidersModuleApiMock = new Mock<ISearchProvidersModuleApi>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _loggerMock = new Mock<ILogger<ServiceActivatedIntegrationEventHandler>>();

        _handler = new ServiceActivatedIntegrationEventHandler(
            _searchProvidersModuleApiMock.Object,
            _providersModuleApiMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceActivated_ShouldReindexProviders()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var providerId1 = Guid.NewGuid();
        var providerId2 = Guid.NewGuid();
        var integrationEvent = new ServiceActivatedIntegrationEvent("ServiceCatalogs", serviceId, "Service Name");

        _providersModuleApiMock.Setup(x => x.GetProvidersByServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<Guid>>.Success(new List<Guid> { providerId1, providerId2 }));

        _searchProvidersModuleApiMock.Setup(x => x.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _searchProvidersModuleApiMock.Verify(x => x.IndexProviderAsync(providerId1, It.IsAny<CancellationToken>()), Times.Once);
        _searchProvidersModuleApiMock.Verify(x => x.IndexProviderAsync(providerId2, It.IsAny<CancellationToken>()), Times.Once);
    }
}
