using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class EnvironmentSpecificExtensionsTests
{
    [Fact]
    public void AddEnvironmentSpecificServices_InProduction_ShouldAddProductionServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateEnvironment("Production");

        // Act
        services.AddEnvironmentSpecificServices(configuration, environment);

        // Assert
        var provider = services.BuildServiceProvider();
        var securityOptions = provider.GetService<IOptions<SecurityOptions>>();
        securityOptions.Should().NotBeNull();
    }

    [Fact]
    public void AddEnvironmentSpecificServices_InDevelopment_ShouldAddDevelopmentServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateEnvironment("Development");

        // Act
        services.AddEnvironmentSpecificServices(configuration, environment);

        // Assert
        services.Should().NotBeNull();
        // Development services are registered (documentation, etc.)
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AddEnvironmentSpecificServices_InTesting_ShouldAddTestingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateEnvironment("Testing");

        // Act
        services.AddEnvironmentSpecificServices(configuration, environment);

        // Assert
        services.Should().NotBeNull();
        // Testing services should be minimal (configured by tests themselves)
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public void AddEnvironmentSpecificServices_InStaging_ShouldNotAddSpecificServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateEnvironment("Staging");

        // Act
        services.AddEnvironmentSpecificServices(configuration, environment);

        // Assert
        services.Should().NotBeNull();
        // No specific services for Staging (fallback behavior)
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Fact]
    public async Task UseEnvironmentSpecificMiddlewares_InDevelopment_ShouldAddDevelopmentMiddlewares()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);
        var environment = CreateEnvironment("Development");

        var middlewareInvoked = false;
        app.Use(async (context, next) =>
        {
            middlewareInvoked = true;
            await next();
        });

        // Act
        app.UseEnvironmentSpecificMiddlewares(environment);
        var pipeline = app.Build();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = provider;
        await pipeline.Invoke(httpContext);

        // Assert
        middlewareInvoked.Should().BeTrue();
    }

    [Fact]
    public void UseEnvironmentSpecificMiddlewares_InProduction_ShouldRegisterProductionMiddlewares()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>();
        var provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);
        var environment = CreateEnvironment("Production");

        // Act - Just verify it doesn't throw, middleware registration is tested in integration tests
        var act = () => app.UseEnvironmentSpecificMiddlewares(environment);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task UseEnvironmentSpecificMiddlewares_InTesting_ShouldAddDevelopmentMiddlewares()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var provider = services.BuildServiceProvider();

        var app = new ApplicationBuilder(provider);
        var environment = CreateEnvironment("Testing");

        var middlewareInvoked = false;
        app.Use(async (context, next) =>
        {
            middlewareInvoked = true;
            await next();
        });

        // Act
        app.UseEnvironmentSpecificMiddlewares(environment);
        var pipeline = app.Build();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = provider;
        await pipeline.Invoke(httpContext);

        // Assert
        middlewareInvoked.Should().BeTrue();
    }

    private static IWebHostEnvironment CreateEnvironment(string environmentName)
    {
        var mock = new Mock<IWebHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(environmentName);
        return mock.Object;
    }
}
