using FluentAssertions;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MeAjudaAi.Modules.Users.Tests.API;

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
    public void AddUsersModule_ShouldReturnServiceCollection()
    {
        // Act
        var result = MeAjudaAi.Modules.Users.API.Extensions.AddUsersModule(_services, _configuration);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }

    [Fact]
    public void AddUsersModule_ShouldRegisterApplicationServices()
    {
        // Act
        MeAjudaAi.Modules.Users.API.Extensions.AddUsersModule(_services, _configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType.Namespace != null &&
                                        s.ServiceType.Namespace.Contains("Users"));
    }

    [Fact]
    public void AddUsersModule_ShouldRegisterDbContext()
    {
        // Act
        MeAjudaAi.Modules.Users.API.Extensions.AddUsersModule(_services, _configuration);

        // Assert
        _services.Should().Contain(s => s.ServiceType == typeof(UsersDbContext));
    }

    [Fact]
    public void UseUsersModule_ShouldReturnWebApplication()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        MeAjudaAi.Modules.Users.API.Extensions.AddUsersModule(serviceCollection, _configuration);
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
        var result = MeAjudaAi.Modules.Users.API.Extensions.UseUsersModule(app);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public async Task AddUsersModuleWithSchemaIsolationAsync_WithSchemaIsolationDisabled_ShouldOnlyRegisterModule()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=test;Username=test;Password=test" },
                { "Database:EnableSchemaIsolation", "false" }
            })
            .Build();

        // Act
        var result = await MeAjudaAi.Modules.Users.API.Extensions.AddUsersModuleWithSchemaIsolationAsync(_services, config);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(_services);
    }
}
