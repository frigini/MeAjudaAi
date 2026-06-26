using MeAjudaAi.Modules.Locations.Domain.Events;
using MeAjudaAi.Modules.Locations.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Locations;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure.Events.Handlers;

public class AllowedCityDomainEventHandlersTests
{
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<AllowedCityCreatedDomainEventHandler>> _loggerCreatedMock = new();
    private readonly Mock<ILogger<AllowedCityUpdatedDomainEventHandler>> _loggerUpdatedMock = new();
    private readonly Mock<ILogger<AllowedCityDeletedDomainEventHandler>> _loggerDeletedMock = new();

    [Fact]
    public async Task CreatedHandler_HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var handler = new AllowedCityCreatedDomainEventHandler(_messageBusMock.Object, _loggerCreatedMock.Object);
        var domainEvent = new AllowedCityCreatedDomainEvent(Guid.NewGuid(), "City", "ST");

        // Act
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<AllowedCityCreatedIntegrationEvent>(e => e.CityId == domainEvent.CityId && e.CityName == "City" && e.StateSigla == "ST"),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatedHandler_HandleAsync_ShouldLogInformation()
    {
        // Arrange
        var handler = new AllowedCityCreatedDomainEventHandler(_messageBusMock.Object, _loggerCreatedMock.Object);
        var domainEvent = new AllowedCityCreatedDomainEvent(Guid.NewGuid(), "City", "ST");

        // Act
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _loggerCreatedMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task CreatedHandler_HandleAsync_WhenPublishFails_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var handler = new AllowedCityCreatedDomainEventHandler(_messageBusMock.Object, _loggerCreatedMock.Object);
        var domainEvent = new AllowedCityCreatedDomainEvent(Guid.NewGuid(), "City", "ST");
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<object>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Pub fail"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => handler.HandleAsync(domainEvent, CancellationToken.None));
        
        // Assert
        _loggerCreatedMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task UpdatedHandler_HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var handler = new AllowedCityUpdatedDomainEventHandler(_messageBusMock.Object, _loggerUpdatedMock.Object);
        var domainEvent = new AllowedCityUpdatedDomainEvent(Guid.NewGuid(), "NewCity", "NS");

        // Act
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<AllowedCityUpdatedIntegrationEvent>(e => e.CityId == domainEvent.CityId && e.CityName == "NewCity" && e.StateSigla == "NS"),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletedHandler_HandleAsync_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var handler = new AllowedCityDeletedDomainEventHandler(_messageBusMock.Object, _loggerDeletedMock.Object);
        var domainEvent = new AllowedCityDeletedDomainEvent(Guid.NewGuid(), "City", "ST");

        // Act
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<AllowedCityDeletedIntegrationEvent>(e => e.CityId == domainEvent.CityId && e.CityName == "City" && e.StateSigla == "ST"),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
