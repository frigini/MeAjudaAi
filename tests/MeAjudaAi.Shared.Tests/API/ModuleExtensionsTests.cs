using FluentAssertions;
using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts.Modules.Documents;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MeAjudaAi.Shared.Tests.API;

public class DocumentsModuleExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public DocumentsModuleExtensionsTests()
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
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
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

public class ProvidersModuleExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ProvidersModuleExtensionsTests()
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
        _services.Should().Contain(s => s.ServiceType == typeof(MeAjudaAi.Modules.Providers.Infrastructure.Persistence.ProvidersDbContext));
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

public class ServiceCatalogsModuleExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ServiceCatalogsModuleExtensionsTests()
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

public class UsersModuleExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public UsersModuleExtensionsTests()
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
        _services.Should().Contain(s => s.ServiceType == typeof(MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext));
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
