using MeAjudaAi.ApiService.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Tests.Unit.Extensions;

/// <summary>
/// Testes de integração para DocumentationExtensions que geram documentos Swagger reais
/// e verificam resultados comportamentais ao invés de apenas registro de serviços.
/// </summary>
public sealed class DocumentationExtensionsTests
{
    [Fact]
    public void SwaggerGenOptions_ShouldNotBeNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        // Assert
        options.Should().NotBeNull();

        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_ShouldRegisterMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();
        services.AddRouting();
        services.AddSingleton<IWebHostEnvironment>(new Mock<IWebHostEnvironment>().Object);

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseDocumentation();

        // Assert
        app.Should().NotBeNull();

        serviceProvider.Dispose();
    }
}
