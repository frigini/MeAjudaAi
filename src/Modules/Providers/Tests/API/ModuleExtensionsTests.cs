using FluentAssertions;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.API;

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
    public void AddProvidersModule_ShouldReturnServiceCollection()
    {
        // Act
        var result = MeAjudaAi.Modules.Providers.API.Extensions.AddProvidersModule(_services, _configuration);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddProvidersModule_ShouldRegisterApplicationServices()
    {
        // Act
        MeAjudaAi.Modules.Providers.API.Extensions.AddProvidersModule(_services, _configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType.Namespace != null &&
                                        s.ServiceType.Namespace.Contains("Providers"));
    }

    [Fact]
    public void AddProvidersModule_ShouldRegisterDbContext()
    {
        // Act
        MeAjudaAi.Modules.Providers.API.Extensions.AddProvidersModule(_services, _configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(ProvidersDbContext));
    }

    [Fact]
    public void UseProvidersModule_ShouldReturnWebApplication()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        MeAjudaAi.Modules.Providers.API.Extensions.AddProvidersModule(serviceCollection, _configuration);
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
        var result = MeAjudaAi.Modules.Providers.API.Extensions.UseProvidersModule(app);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }
}
