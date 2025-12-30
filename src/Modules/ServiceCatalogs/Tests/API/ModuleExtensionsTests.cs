using FluentAssertions;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.API;

public class ModuleExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ModuleExtensionsTests()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=test;Password=test" }
            })
            .Build();
    }

    [Fact]
    public void AddServiceCatalogsModule_ShouldReturnServiceCollection()
    {
        // Act
        var result = MeAjudaAi.Modules.ServiceCatalogs.API.Extensions.AddServiceCatalogsModule(_services, _configuration);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddServiceCatalogsModule_ShouldRegisterApplicationServices()
    {
        // Act
        MeAjudaAi.Modules.ServiceCatalogs.API.Extensions.AddServiceCatalogsModule(_services, _configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType.Namespace != null &&
                                        s.ServiceType.Namespace.Contains("ServiceCatalogs"));
    }

    [Fact]
    public void AddServiceCatalogsModule_ShouldRegisterModuleApi()
    {
        // Act
        MeAjudaAi.Modules.ServiceCatalogs.API.Extensions.AddServiceCatalogsModule(_services, _configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(IServiceCatalogsModuleApi));
    }

    [Fact]
    public void UseServiceCatalogsModule_ShouldReturnWebApplication()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        MeAjudaAi.Modules.ServiceCatalogs.API.Extensions.AddServiceCatalogsModule(serviceCollection, _configuration);
        serviceCollection.AddLogging();
        serviceCollection.AddRouting();

        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.EnvironmentName).Returns("Testing");
        serviceCollection.AddSingleton(envMock.Object);

        var appBuilder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        foreach (var service in serviceCollection)
        {
            appBuilder.Services.Add(service);
        }
        var app = appBuilder.Build();

        // Act
        var result = MeAjudaAi.Modules.ServiceCatalogs.API.Extensions.UseServiceCatalogsModule(app);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }
}
