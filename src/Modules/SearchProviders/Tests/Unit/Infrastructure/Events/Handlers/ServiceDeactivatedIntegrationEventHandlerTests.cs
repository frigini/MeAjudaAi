using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

public class ServiceDeactivatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchableProviderRepository> _repositoryMock;
    private readonly Mock<ILogger<ServiceDeactivatedIntegrationEventHandler>> _loggerMock;
    private readonly ServiceDeactivatedIntegrationEventHandler _handler;

    public ServiceDeactivatedIntegrationEventHandlerTests()
    {
        _repositoryMock = new Mock<ISearchableProviderRepository>();
        _loggerMock = new Mock<ILogger<ServiceDeactivatedIntegrationEventHandler>>();
        _handler = new ServiceDeactivatedIntegrationEventHandler(_repositoryMock.Object, _loggerMock.Object);
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

        _repositoryMock.Setup(x => x.GetByServiceIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchableProvider> { provider });

        // Act
        await _handler.HandleAsync(integrationEvent);

        // Assert
        provider.ServiceIds.Should().NotContain(serviceId);
        _repositoryMock.Verify(x => x.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
