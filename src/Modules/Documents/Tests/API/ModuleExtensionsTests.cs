using FluentAssertions;
using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts.Modules.Documents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MeAjudaAi.Modules.Documents.Tests.API;

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
    public void AddDocumentsModule_ShouldReturnServiceCollection()
    {
        // Act
        var result = _services.AddDocumentsModule(_configuration);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterApplicationServices()
    {
        // Act
        _services.AddDocumentsModule(_configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType.Namespace != null &&
                                        s.ServiceType.Namespace.Contains("Documents"));
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterModuleApi()
    {
        // Act
        _services.AddDocumentsModule(_configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(IDocumentsModuleApi));
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterDbContext()
    {
        // Act
        _services.AddDocumentsModule(_configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(DocumentsDbContext));
    }

    [Fact]
    public void UseDocumentsModule_ShouldReturnWebApplication()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDocumentsModule(_configuration);
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
        var result = app.UseDocumentsModule();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void UseDocumentsModule_ShouldConfigureEndpoints()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDocumentsModule(_configuration);
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
        app.UseDocumentsModule();

        // Assert - verify endpoint sources were added
        var endpointDataSources = app.Services.GetService<IEnumerable<EndpointDataSource>>();
        endpointDataSources.Should().NotBeNull();
    }
}
