using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.SearchProviders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure.Events.Handlers;

public class AllowedCityUpdatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchModuleApiMock = new();
    private readonly Mock<ISearchableProviderQueries> _queriesMock = new();
    private readonly Mock<ILogger<AllowedCityUpdatedIntegrationEventHandler>> _loggerMock = new();

    [Fact]
    public async Task HandleAsync_WhenAllowedCityUpdated_WithProviders_ShouldReindexAllProviders()
    {
        // Arrange
        var handler = new AllowedCityUpdatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _queriesMock.Object,
            _loggerMock.Object);

        var evt = new AllowedCityUpdatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        var providers = new List<SearchableProvider>
        {
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build()
        };

        _queriesMock.Setup(q => q.GetByCityIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        _searchModuleApiMock.Setup(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchModuleApiMock.Verify(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAllowedCityUpdated_WithNoProviders_ShouldNotCallIndexProvider()
    {
        // Arrange
        var handler = new AllowedCityUpdatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _queriesMock.Object,
            _loggerMock.Object);

        var evt = new AllowedCityUpdatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        _queriesMock.Setup(q => q.GetByCityIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchableProvider>());

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchModuleApiMock.Verify(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenIndexFails_ShouldLogErrorAndContinue()
    {
        // Arrange
        var handler = new AllowedCityUpdatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _queriesMock.Object,
            _loggerMock.Object);

        var evt = new AllowedCityUpdatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        var providers = new List<SearchableProvider>
        {
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build(),
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build()
        };

        _queriesMock.Setup(q => q.GetByCityIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var callCount = 0;
        _searchModuleApiMock.Setup(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    return Result.Failure(Error.BadRequest("Index failed"));

                return Result.Success();
            });

        // Act — should not throw even though one provider fails
        await handler.HandleAsync(evt);

        // Assert — both providers attempted
        _searchModuleApiMock.Verify(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to reindex provider")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
