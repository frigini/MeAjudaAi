using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure;

public class ExtensionsTests
{
    [Fact]
    public void AddEventHandlers_ShouldRegisterAllDomainEventHandlers()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddLogging();
        
        // Registrar dependências necessárias para AddInfrastructure
        services.AddScoped(sp => new Mock<MeAjudaAi.Shared.Database.Abstractions.IUnitOfWork>().Object);

        MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Extensions.AddServiceCatalogsInfrastructure(services, configuration);
        var provider = services.BuildServiceProvider();

        provider.GetService<IEventHandler<ServiceActivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceDeactivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceUpdatedDomainEvent>>().Should().NotBeNull();
    }
}
