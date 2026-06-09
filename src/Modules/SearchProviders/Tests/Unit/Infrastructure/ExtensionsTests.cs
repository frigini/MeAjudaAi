using MeAjudaAi.Shared.Events;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Messaging.Messages.Ratings;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.Infrastructure;

public class ExtensionsTests
{
    [Fact]
    public void AddEventHandlers_ShouldRegisterAllIntegrationEventHandlers()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test"
            })
            .Build();
        var hostEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment>();
        services.AddLogging();
        
        services.AddScoped(sp => new Mock<MeAjudaAi.Shared.Database.Abstractions.IUnitOfWork>().Object);
        services.AddScoped(sp => new Mock<MeAjudaAi.Shared.Database.Abstractions.IDapperConnection>().Object);
        services.AddScoped(sp => new Mock<MeAjudaAi.Contracts.Modules.SearchProviders.ISearchProvidersModuleApi>().Object);
        services.AddScoped(sp => new Mock<MeAjudaAi.Contracts.Modules.Providers.IProvidersModuleApi>().Object);

        MeAjudaAi.Modules.SearchProviders.Infrastructure.Extensions.AddSearchProvidersInfrastructure(services, configuration, hostEnv.Object);

        var provider = services.BuildServiceProvider();

        provider.GetService<IEventHandler<ProviderActivatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderDeletedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderIndexRequiredIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderProfileUpdatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ProviderServicesUpdatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ReviewApprovedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceActivatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceDeactivatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<AllowedCityCreatedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<AllowedCityDeletedIntegrationEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<AllowedCityUpdatedIntegrationEvent>>().Should().NotBeNull();
    }
}
