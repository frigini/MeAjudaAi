using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "Infrastructure")]
public class ServiceDeactivatedIntegrationEventHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ISearchableProviderQueries> _queriesMock;
    private readonly Mock<ILogger<ServiceDeactivatedIntegrationEventHandler>> _loggerMock;
    private readonly ServiceDeactivatedIntegrationEventHandler _handler;

    public ServiceDeactivatedIntegrationEventHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _queriesMock = new Mock<ISearchableProviderQueries>();
        _loggerMock = new Mock<ILogger<ServiceDeactivatedIntegrationEventHandler>>();
        _handler = new ServiceDeactivatedIntegrationEventHandler(_uowMock.Object, _queriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceDeactivated_ShouldRemoveFromProviders()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var integrationEvent = new ServiceDeactivatedIntegrationEvent("ServiceCatalogs", serviceId);

        var provider = SearchableProvider.Create(
            Guid.NewGuid(), "Provider 1", "p1", new GeoPoint(0, 0));
        provider.UpdateServices([serviceId, Guid.NewGuid()]);

        _queriesMock.Setup(x => x.GetByServiceIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchableProvider> { provider });

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        provider.ServiceIds.Should().NotContain(serviceId);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoProvidersFound_ShouldDoNothing()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var integrationEvent = new ServiceDeactivatedIntegrationEvent("ServiceCatalogs", serviceId);

        _queriesMock.Setup(x => x.GetByServiceIdAsync(serviceId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchableProvider>());

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

