using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

public class AllowedCityIntegrationEventHandlerTests
{
    private readonly Mock<ILogger<AllowedCityCreatedIntegrationEventHandler>> _loggerCreatedMock = new();
    private readonly Mock<ILogger<AllowedCityUpdatedIntegrationEventHandler>> _loggerUpdatedMock = new();
    private readonly Mock<ILogger<AllowedCityDeletedIntegrationEventHandler>> _loggerDeletedMock = new();

    [Fact]
    public async Task HandleAsync_WhenAllowedCityCreated_ShouldLogAndComplete()
    {
        var handler = new AllowedCityCreatedIntegrationEventHandler(_loggerCreatedMock.Object);
        var evt = new AllowedCityCreatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        await handler.HandleAsync(evt);

        _loggerCreatedMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling AllowedCityCreated")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAllowedCityUpdated_ShouldLogAndComplete()
    {
        var handler = new AllowedCityUpdatedIntegrationEventHandler(_loggerUpdatedMock.Object);
        var evt = new AllowedCityUpdatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        await handler.HandleAsync(evt);

        _loggerUpdatedMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling AllowedCityUpdated")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAllowedCityDeleted_ShouldLogAndComplete()
    {
        var handler = new AllowedCityDeletedIntegrationEventHandler(_loggerDeletedMock.Object);
        var evt = new AllowedCityDeletedIntegrationEvent("Locations", Guid.NewGuid());

        await handler.HandleAsync(evt);

        _loggerDeletedMock.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling AllowedCityDeleted")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
