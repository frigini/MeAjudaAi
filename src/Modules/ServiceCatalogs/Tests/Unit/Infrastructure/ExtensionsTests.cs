using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        services.AddScoped(sp => new Mock<IUnitOfWork>().Object);
        // Mock do MessageBus para evitar falha de resolução
        services.AddSingleton<IMessageBus>(new Mock<IMessageBus>().Object);

        // Mock para DbContext
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        services.AddScoped(sp => new ServiceCatalogsDbContext(options));

        var environment = new Mock<IHostEnvironment>();
        Extensions.AddInfrastructure(services, configuration, environment.Object);
        var provider = services.BuildServiceProvider();

        provider.GetService<IEventHandler<ServiceActivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceDeactivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceUpdatedDomainEvent>>().Should().NotBeNull();
    }
}
