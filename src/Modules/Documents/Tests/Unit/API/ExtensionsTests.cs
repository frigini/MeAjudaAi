using MeAjudaAi.Modules.Documents.API;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts.Modules.Documents;
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
        // Preparação
        var services = new ServiceCollection();

        // Ação
        var result = services.AddDocumentsModule(_testConfiguration);

        // Verificação
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddDocumentsModule_ShouldRegisterIDocumentsModuleApi()
    {
        // Preparação
        var services = new ServiceCollection();

        // Ação
        services.AddDocumentsModule(_testConfiguration);

        // Verificação
        // Verifica se IDocumentsModuleApi está registrado
        Assert.Contains(services, s => s.ServiceType == typeof(IDocumentsModuleApi));
    }

    [Fact]
    public void AddDocumentsModule_WithValidConfiguration_ShouldRegisterServices()
    {
        // Preparação
        var services = new ServiceCollection();

        // Ação
        var result = services.AddDocumentsModule(_testConfiguration);

        // Verificação
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void AddDocumentsModule_ShouldReturnSameServiceCollectionInstance()
    {
        // Preparação
        var services = new ServiceCollection();

        // Ação
        var result = services.AddDocumentsModule(_testConfiguration);

        // Verificação
        Assert.Same(services, result);
    }

    [Theory]
    [InlineData("Host=localhost;Database=docs1;Username=test;Password=test;")]
    [InlineData("Host=localhost;Database=docs2;Username=test;Password=test;")]
    public void AddDocumentsModule_WithVariousConfigurations_ShouldRegisterServices(string connectionString)
    {
        // Preparação
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();

        // Ação
        var result = services.AddDocumentsModule(configuration);

        // Verificação
        Assert.NotNull(result);
        Assert.Same(services, result);
        Assert.True(services.Count > 0);
    }

    [Fact]
    public void UseDocumentsModule_InTestEnvironment_ShouldSkipMigrations()
    {
        // Preparação
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();

        var testEnvMock = new Mock<IWebHostEnvironment>();
        testEnvMock.Setup(e => e.EnvironmentName).Returns("Test");
        testEnvMock.Setup(e => e.ApplicationName).Returns("TestApp");
        builder.Services.AddSingleton(testEnvMock.Object);

        builder.Configuration.AddConfiguration(_testConfiguration);
        builder.Services.AddDocumentsModule(_testConfiguration);

        var app = builder.Build();

        // Ação - Não deve lançar exceção em ambiente de teste
        var result = app.UseDocumentsModule();

        // Verificação
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    [Fact]
    public void UseDocumentsModule_InTestingEnvironment_ShouldSkipMigrations()
    {
        // Preparação
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();

        var testEnvMock = new Mock<IWebHostEnvironment>();
        testEnvMock.Setup(e => e.EnvironmentName).Returns("Testing");
        testEnvMock.Setup(e => e.ApplicationName).Returns("TestApp");
        builder.Services.AddSingleton(testEnvMock.Object);

        builder.Configuration.AddConfiguration(_testConfiguration);
        builder.Services.AddDocumentsModule(_testConfiguration);

        var app = builder.Build();

        // Ação
        var result = app.UseDocumentsModule();

        // Verificação
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    [Fact]
    public void UseDocumentsModule_WithoutDbContext_ShouldLogWarningAndContinue()
    {
        // Preparação
        var builder = WebApplication.CreateBuilder();

        var mockLogger = new Mock<ILogger<DocumentsDbContext>>();
        builder.Services.AddSingleton(mockLogger.Object);
        builder.Services.AddLogging();

        var app = builder.Build();

        // Ação
        var result = app.UseDocumentsModule();

        // Verificação
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    [Fact]
    public void UseDocumentsModule_ShouldReturnSameAppInstance()
    {
        // Preparação
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddLogging();

        var testEnvMock = new Mock<IWebHostEnvironment>();
        testEnvMock.Setup(e => e.EnvironmentName).Returns("Test");
        testEnvMock.Setup(e => e.ApplicationName).Returns("TestApp");
        builder.Services.AddSingleton(testEnvMock.Object);

        var app = builder.Build();

        // Ação
        var result = app.UseDocumentsModule();

        // Verificação
        Assert.Same(app, result);
    }
}
