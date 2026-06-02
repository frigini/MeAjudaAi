using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
public class ServiceUpdatedDomainEventHandlerTests
{
    private readonly Mock<IServiceQueries> _serviceQueriesMock = new();
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<ServiceUpdatedDomainEventHandler>> _loggerMock = new();

    [Fact]
    public async Task ServiceUpdatedHandler_Should_PublishIntegrationEvent()
    {
        // Arrange
        var service = Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Test Service", null, 0);
        _serviceQueriesMock.Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var handler = new ServiceUpdatedDomainEventHandler(_serviceQueriesMock.Object, _messageBusMock.Object, _loggerMock.Object);
        var domainEvent = new ServiceUpdatedDomainEvent(service.Id);

        // Act
        await handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(x => x.PublishAsync(
            It.Is<ServiceNameUpdatedIntegrationEvent>(e => 
                e.ServiceId == service.Id.Value && 
                e.NewName == service.Name && 
                e.Source == ModuleNames.ServiceCatalogs), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
