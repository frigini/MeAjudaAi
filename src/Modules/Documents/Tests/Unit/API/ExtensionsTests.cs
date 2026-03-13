using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Services;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Contracts.Modules.Documents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.API;

/// <summary>
/// Testes unitários para os métodos de extensão do módulo Documents
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Documents")]
[Trait("Layer", "API")]
public sealed class ExtensionsTests
{
    private readonly IConfiguration _testConfiguration;

    public ExtensionsTests()
    {
        _testConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test;"
            })
            .Build();
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var result = services.AddDocumentsModule(_testConfiguration, mockEnv.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterIDocumentsModuleApi()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        services.AddDocumentsModule(_testConfiguration, mockEnv.Object);

        // Assert
        // Verifica se IDocumentsModuleApi está registrado
        Assert.Contains(services, s => s.ServiceType == typeof(IDocumentsModuleApi));
    }

    [Fact]
    public void AddDocumentsModule_WithValidConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var result = services.AddDocumentsModule(_testConfiguration, mockEnv.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddDocumentsModule_ShouldReturnSameServiceCollectionInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var result = services.AddDocumentsModule(_testConfiguration, mockEnv.Object);

        // Assert
        Assert.Same(services, result);
    }

    [Theory]
    [InlineData("Host=localhost;Database=docs1;Username=test;Password=test;")]
    [InlineData("Host=localhost;Database=docs2;Username=test;Password=test;")]
    public void AddDocumentsModule_WithVariousConfigurations_ShouldRegisterServices(string connectionString)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();

        // Act
        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var result = services.AddDocumentsModule(configuration, mockEnv.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void UseDocumentsModule_InTestEnvironment_ShouldSkipMigrations()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();

        var testEnvMock = new Mock<IWebHostEnvironment>();
        testEnvMock.Setup(e => e.EnvironmentName).Returns("Testing");
        testEnvMock.Setup(e => e.ApplicationName).Returns("TestApp");
        builder.Services.AddSingleton(testEnvMock.Object);

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        builder.Services.AddDocumentsModule(_testConfiguration, mockEnv.Object);

        var app = builder.Build();

        // Act - Não deve lançar exceção em ambiente de teste
        var result = app.UseDocumentsModule();

        // Assert
        Assert.NotNull(result);
        Assert.Same(app, result);
    }


    [Fact]
    public void UseDocumentsModule_WithoutDbContext_ShouldLogWarningAndContinue()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        var mockLogger = new Mock<ILogger<DocumentsDbContext>>();
        builder.Services.AddSingleton(mockLogger.Object);
        builder.Services.AddLogging();

        var app = builder.Build();

        // Act
        var result = app.UseDocumentsModule();

        // Assert
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    [Fact]
    public void AddDocumentsModule_InProduction_WithFullConfiguration_ShouldRegisterRealServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;",
                ["Azure:Storage:ConnectionString"] = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;EndpointSuffix=core.windows.net",
                ["Azure:DocumentIntelligence:Endpoint"] = "https://test.cognitiveservices.azure.com/",
                ["Azure:DocumentIntelligence:ApiKey"] = "test-key"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        services.AddDocumentsModule(configuration, mockEnv.Object);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(IBlobStorageService) && s.ImplementationType == typeof(AzureBlobStorageService));
        Assert.Contains(services, s => s.ServiceType == typeof(IDocumentIntelligenceService) && s.ImplementationType == typeof(AzureDocumentIntelligenceService));
    }

    [Fact]
    public void AddDocumentsModule_WithoutConnectionStrings_InProduction_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var act = () => services.AddDocumentsModule(configuration, mockEnv.Object);

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("Database connection string is not configured", ex.Message);
    }

    [Fact]
    public void AddDocumentsModule_InProduction_MissingStorage_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;",
                ["Azure:DocumentIntelligence:Endpoint"] = "https://test.cognitiveservices.azure.com/",
                ["Azure:DocumentIntelligence:ApiKey"] = "test-key"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var act = () => services.AddDocumentsModule(configuration, mockEnv.Object);

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("IBlobStorageService is not configured", ex.Message);
    }

    [Fact]
    public void AddDocumentsModule_InProduction_MissingIntelligence_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;",
                ["Azure:Storage:ConnectionString"] = "DefaultEndpointsProtocol=https;AccountName=test;AccountKey=test;EndpointSuffix=core.windows.net"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Production);

        // Act
        var act = () => services.AddDocumentsModule(configuration, mockEnv.Object);

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Contains("IDocumentIntelligenceService is not configured", ex.Message);
    }

    [Fact]
    public void AddDocumentsModule_WithMinimalConnectionStrings_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:meajudaai-db"] = "Host=localhost;Database=test;"
            })
            .Build();

        var mockEnv = new Mock<IHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns(Environments.Development);

        // Act
        services.AddDocumentsModule(configuration, mockEnv.Object);

        // Assert
        Assert.Contains(services, s => s.ServiceType == typeof(DocumentsDbContext));
    }
}
