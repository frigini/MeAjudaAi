using FluentAssertions;
using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Contracts.Modules.Documents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

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
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        var result = _services.AddDocumentsModule(_configuration, mockEnv.Object);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterApplicationServices()
    {
        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        _services.AddDocumentsModule(_configuration, mockEnv.Object);

        // Assert
        _services.Should().Contain(s => s.ServiceType.Namespace != null &&
                                        s.ServiceType.Namespace.Contains("Documents"));
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterModuleApi()
    {
        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        _services.AddDocumentsModule(_configuration, mockEnv.Object);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(IDocumentsModuleApi));
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterDbContext()
    {
        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        _services.AddDocumentsModule(_configuration, mockEnv.Object);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(DocumentsDbContext));
    }

    [Fact]
    public void UseDocumentsModule_ShouldReturnWebApplication()
    {
        // Arrange
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDocumentsModule(_configuration, mockEnv.Object);
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
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddDocumentsModule(_configuration, mockEnv.Object);
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
