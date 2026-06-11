using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

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
        // Mock do MessageBus para evitar falha de resolução
        services.AddSingleton<MeAjudaAi.Shared.Messaging.IMessageBus>(new Mock<MeAjudaAi.Shared.Messaging.IMessageBus>().Object);

        // Mock para DbContext
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        services.AddScoped(sp => new MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.ServiceCatalogsDbContext(options));

        var environment = new Mock<IHostEnvironment>();
        MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Extensions.AddInfrastructure(services, configuration, environment.Object);
        var provider = services.BuildServiceProvider();

        provider.GetService<IEventHandler<ServiceActivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceDeactivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceUpdatedDomainEvent>>().Should().NotBeNull();
    }
}
