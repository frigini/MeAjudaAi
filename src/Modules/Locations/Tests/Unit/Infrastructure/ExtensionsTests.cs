using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Events;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
[Trait("Module", "Locations")]
[Trait("Layer", "Infrastructure")]
public class ExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DatabaseConstants.DefaultTestConnectionString
            })
            .Build();
        services.AddSingleton<IConfiguration>(configuration);
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddLogging();
        services.AddSingleton(new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>().Object);

        // Act
        MeAjudaAi.Modules.Locations.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);

        // Assert - verify registrations exist in the collection
        services.Should().Contain(s => s.ServiceType == typeof(IAllowedCityQueries));
        services.Should().Contain(s => s.ServiceType == typeof(ICepLookupService));
        services.Should().Contain(s => s.ServiceType == typeof(IGeocodingService));
        services.Should().Contain(s => s.ServiceType == typeof(IIbgeService));
        services.Should().Contain(s => s.ServiceType == typeof(IGeographicValidationService));
        services.Should().Contain(s => s.ServiceType == typeof(ILocationsModuleApi));

        // Assert - event handlers
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<AllowedCityCreatedDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<AllowedCityUpdatedDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<AllowedCityDeletedDomainEvent>));
    }
}
