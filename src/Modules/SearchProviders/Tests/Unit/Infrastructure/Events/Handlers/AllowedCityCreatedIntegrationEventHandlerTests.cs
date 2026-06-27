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

public class AllowedCityCreatedIntegrationEventHandlerTests
{
    private readonly Mock<ISearchProvidersModuleApi> _searchModuleApiMock = new();
    private readonly Mock<ISearchableProviderQueries> _queriesMock = new();
    private readonly Mock<ILogger<AllowedCityCreatedIntegrationEventHandler>> _loggerMock = new();

    [Fact]
    public async Task HandleAsync_WhenAllowedCityCreated_WithProviders_ShouldReindexAllProviders()
    {
        // Arrange
        var handler = new AllowedCityCreatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _queriesMock.Object,
            _loggerMock.Object);

        var evt = new AllowedCityCreatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        var providers = new List<SearchableProvider>
        {
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build(),
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build()
        };

        _queriesMock.Setup(q => q.GetByCityAndStateSiglaAsync("Muriaé", "MG", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        _searchModuleApiMock.Setup(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchModuleApiMock.Verify(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleAsync_WhenAllowedCityCreated_WithNoProviders_ShouldNotCallIndexProvider()
    {
        // Arrange
        var handler = new AllowedCityCreatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _queriesMock.Object,
            _loggerMock.Object);

        var evt = new AllowedCityCreatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        _queriesMock.Setup(q => q.GetByCityAndStateSiglaAsync("Muriaé", "MG", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SearchableProvider>());

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchModuleApiMock.Verify(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenIndexFails_ShouldContinueProcessingOtherProviders()
    {
        // Arrange
        var handler = new AllowedCityCreatedIntegrationEventHandler(
            _searchModuleApiMock.Object,
            _queriesMock.Object,
            _loggerMock.Object);

        var evt = new AllowedCityCreatedIntegrationEvent("Locations", Guid.NewGuid(), "Muriaé", "MG");

        var providers = new List<SearchableProvider>
        {
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build(),
            new SearchableProviderBuilder().WithCity("Muriaé").WithState("MG").Build()
        };

        _queriesMock.Setup(q => q.GetByCityAndStateSiglaAsync("Muriaé", "MG", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        var callCount = 0;
        _searchModuleApiMock.Setup(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                    return Result.Failure("Index failed");
                return Result.Success();
            });

        // Act
        await handler.HandleAsync(evt);

        // Assert
        _searchModuleApiMock.Verify(s => s.IndexProviderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
