using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
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
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=meajudaai_test;Username=postgres;Password=test"
            })
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.Setup(e => e.EnvironmentName).Returns("Testing");
        services.AddLogging();
        services.AddSingleton(new Mock<IMessageBus>().Object);

        // Act
        Extensions.AddInfrastructure(services, configuration, environment.Object);

        // Assert - Queries
        services.Should().Contain(s => s.ServiceType == typeof(IServiceCategoryQueries));
        services.Should().Contain(s => s.ServiceType == typeof(IServiceQueries));

        // Assert - Event Handlers
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<ServiceActivatedDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<ServiceDeactivatedDomainEvent>));
        services.Should().Contain(s => s.ServiceType == typeof(IEventHandler<ServiceUpdatedDomainEvent>));

        // Assert - Command Handlers
        services.Should().Contain(s => s.ServiceType == typeof(ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory.CreateServiceCategoryCommand, Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryDto>>));
        services.Should().Contain(s => s.ServiceType == typeof(ICommandHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service.CreateServiceCommand, Contracts.Functional.Result<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceDto>>));

        // Assert - Query Handlers
        services.Should().Contain(s => s.ServiceType == typeof(IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory.GetAllServiceCategoriesQuery, Contracts.Functional.Result<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceCategoryDto>>>));
        services.Should().Contain(s => s.ServiceType == typeof(IQueryHandler<MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service.GetAllServicesQuery, Contracts.Functional.Result<IReadOnlyList<MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.ServiceListDto>>>));
    }

    [Fact]
    public void AddEventHandlers_ShouldRegisterAllDomainEventHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        services.AddLogging();
        services.AddSingleton(new Mock<IMessageBus>().Object);

        var options = new DbContextOptionsBuilder<ServiceCatalogsDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        services.AddScoped(sp => new ServiceCatalogsDbContext(options));

        // Act
        var environment = new Mock<IHostEnvironment>();
        Extensions.AddInfrastructure(services, configuration, environment.Object);
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IEventHandler<ServiceActivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceDeactivatedDomainEvent>>().Should().NotBeNull();
        provider.GetService<IEventHandler<ServiceUpdatedDomainEvent>>().Should().NotBeNull();
    }
}
