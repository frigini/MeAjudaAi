using MeAjudaAi.Modules.SearchProviders.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Unit.API;

/// <summary>
/// Testes unitários para os métodos de extensão do módulo SearchProviders
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "SearchProviders")]
[Trait("Layer", "API")]
public sealed class ModuleExtensionsTests
{
    [Fact]
    public void AddSearchProvidersModule_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = services.AddSearchProvidersModule(configuration, mockEnv.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddSearchProvidersModule_ShouldReturnSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = services.AddSearchProvidersModule(configuration, mockEnv.Object);

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddSearchProvidersModule_WithEmptyConfiguration_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddSearchProvidersModule(configuration, mockEnv.Object));

        Assert.Contains("Database connection string not found", exception.Message);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void AddSearchProvidersModule_InVariousEnvironments_ShouldRegisterServices(string environmentName)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(environmentName);

        // Act
        var result = services.AddSearchProvidersModule(configuration, mockEnv.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Theory]
    [InlineData("Host=localhost;Database=sp1;Username=test;Password=test;")]
    [InlineData("Host=localhost;Database=sp2;Username=test;Password=test;")]
    public void AddSearchProvidersModule_WithVariousConfigurations_ShouldRegisterServices(string connectionString)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        var result = services.AddSearchProvidersModule(configuration, mockEnv.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddSearchProvidersModule_ShouldRegisterApplicationServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        services.AddSearchProvidersModule(configuration, mockEnv.Object);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);

        // Verifica se alguns serviços básicos estão registrados
        Assert.Contains(services, s => s.ServiceType.Namespace?.Contains("SearchProviders") == true);
    }

    [Fact]
    public void UseSearchProvidersModule_ShouldReturnSameBuilderInstance()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseSearchProvidersModule();

        // Assert
        Assert.Same(app, result);
    }

    [Fact]
    public void UseSearchProvidersModule_ShouldMapEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Test");

        builder.Configuration.AddConfiguration(configuration);
        builder.Services.AddSearchProvidersModule(configuration, mockEnv.Object);

        var app = builder.Build();

        // Act - Não deve lançar exceção
        var result = app.UseSearchProvidersModule();

        // Assert
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    [Fact]
    public void UseSearchProvidersModule_WithMockRouteBuilder_ShouldNotThrow()
    {
        // Arrange
        var mockBuilder = new Mock<IEndpointRouteBuilder>();
        mockBuilder.Setup(b => b.ServiceProvider).Returns(new ServiceCollection().BuildServiceProvider());
        mockBuilder.Setup(b => b.DataSources).Returns(new List<EndpointDataSource>());

        // Act
        var result = mockBuilder.Object.UseSearchProvidersModule();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AddSearchProvidersModule_MultipleInvocations_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

        // Act - Chamar múltiplas vezes não deve lançar exceção (mesmo que não seja ideal)
        services.AddSearchProvidersModule(configuration, mockEnv.Object);
        var exception = Record.Exception(() => services.AddSearchProvidersModule(configuration, mockEnv.Object));

        // Assert
        Assert.Null(exception);
    }
}
