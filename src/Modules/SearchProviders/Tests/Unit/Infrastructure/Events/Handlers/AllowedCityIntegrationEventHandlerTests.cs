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
    public async Task HandleAsync_WhenAllowedCityCreated_ShouldNotThrow()
    {
        var handler = new AllowedCityCreatedIntegrationEventHandler(_loggerCreatedMock.Object);
        var evt = new AllowedCityCreatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenAllowedCityUpdated_ShouldNotThrow()
    {
        var handler = new AllowedCityUpdatedIntegrationEventHandler(_loggerUpdatedMock.Object);
        var evt = new AllowedCityUpdatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleAsync_WhenAllowedCityDeleted_ShouldNotThrow()
    {
        var handler = new AllowedCityDeletedIntegrationEventHandler(_loggerDeletedMock.Object);
        var evt = new AllowedCityDeletedIntegrationEvent("Locations", Guid.NewGuid());

        Func<Task> act = () => handler.HandleAsync(evt);

        await act.Should().NotThrowAsync();
    }
}
