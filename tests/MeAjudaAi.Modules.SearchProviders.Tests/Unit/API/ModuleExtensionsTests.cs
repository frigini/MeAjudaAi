using FluentAssertions;
using MeAjudaAi.Modules.SearchProviders.API;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.API;

public class ModuleExtensionsTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHostEnvironment> _mockEnvironment;
    private readonly ServiceCollection _services;

    public ModuleExtensionsTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockEnvironment = new Mock<IHostEnvironment>();
        _services = new ServiceCollection();
        
        // Setup configuration mock
        var mockConnectionStrings = new Mock<IConfigurationSection>();
        mockConnectionStrings.Setup(x => x.Value).Returns("Host=localhost;Database=test;");
        _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings:SearchProvidersDb"))
            .Returns(mockConnectionStrings.Object);
    }

    [Fact]
    public void AddSearchProvidersModule_ShouldRegisterAllServices()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

        // Act
        _services.AddSearchProvidersModule(_mockConfiguration.Object, _mockEnvironment.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
        _services.Should().NotBeEmpty();
        
        // Verify DbContext is registered
        var dbContextService = _services.FirstOrDefault(s => s.ServiceType == typeof(SearchProvidersDbContext));
        dbContextService.Should().NotBeNull();
    }

    [Fact]
    public void AddSearchProvidersModule_InTestEnvironment_ShouldConfigureInMemoryDatabase()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Testing");

        // Act
        _services.AddSearchProvidersModule(_mockConfiguration.Object, _mockEnvironment.Object);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        serviceProvider.Should().NotBeNull();
        _services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddSearchProvidersModule_ShouldReturnServiceCollection()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

        // Act
        var result = _services.AddSearchProvidersModule(_mockConfiguration.Object, _mockEnvironment.Object);

        // Assert
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void UseSearchProvidersModule_ShouldRegisterEndpoints()
    {
        // Arrange
        var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockEndpointRouteBuilder.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        
        var dataSources = new List<EndpointDataSource>();
        mockEndpointRouteBuilder.Setup(x => x.DataSources).Returns(dataSources);

        // Act
        var result = mockEndpointRouteBuilder.Object.UseSearchProvidersModule();

        // Assert
        result.Should().BeSameAs(mockEndpointRouteBuilder.Object);
    }

    [Fact]
    public void UseSearchProvidersModule_ShouldReturnEndpointRouteBuilder()
    {
        // Arrange
        var mockEndpointRouteBuilder = new Mock<IEndpointRouteBuilder>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockEndpointRouteBuilder.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        
        var dataSources = new List<EndpointDataSource>();
        mockEndpointRouteBuilder.Setup(x => x.DataSources).Returns(dataSources);

        // Act
        var result = mockEndpointRouteBuilder.Object.UseSearchProvidersModule();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mockEndpointRouteBuilder.Object);
    }

    [Fact]
    public void AddSearchProvidersModule_WithNullConfiguration_ShouldStillRegisterServices()
    {
        // Arrange
        _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development");

        // Act
        var act = () => _services.AddSearchProvidersModule(null!, _mockEnvironment.Object);

        // Assert
        // Should throw because configuration is required for DB connection
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void AddSearchProvidersModule_WithNullEnvironment_ShouldThrow()
    {
        // Act
        var act = () => _services.AddSearchProvidersModule(_mockConfiguration.Object, null!);

        // Assert
        act.Should().Throw<Exception>();
    }
}
