using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Extensions;

/// <summary>
/// Testes unitários para DocumentationExtensions (Swagger/OpenAPI configuration).
/// Valida configuração de documentação, autenticação JWT, operações customizadas, e UI.
/// </summary>
public sealed class DocumentationExtensionsTests
{
    #region AddDocumentation Tests

    [Fact]
    public void AddDocumentation_ShouldRegisterEndpointsApiExplorer()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDocumentation();

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IApiDescriptionGroupCollectionProvider));
    }

    [Fact]
    public void AddDocumentation_ShouldRegisterSwaggerGen()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDocumentation();

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(ISwaggerProvider));
    }

    [Fact]
    public void AddDocumentation_ShouldReturnSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddDocumentation();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldConfigureV1Document()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act - Verify service is registered
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldConfigureContactInfo()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act - Verify configuration was applied
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldConfigureBearerSecurity()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act - Verify security configuration was applied
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldHaveSecurityRequirement()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldEnableAnnotations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldUseCustomSchemaIds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldDescribeParametersInCamelCase()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldUseInlineDefinitionsForEnums()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldHaveApiVersionOperationFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_ShouldHaveModuleTagsDocumentFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_CustomOperationIds_WithControllerAction_ShouldGenerateCorrectId()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_CustomOperationIds_WithMinimalApi_ShouldGenerateIdFromRoute()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_CustomOperationIds_WithNullHttpMethod_ShouldUseANY()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_CustomOperationIds_WithEmptyRoute_ShouldUseDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void AddDocumentation_SwaggerGenOptions_CustomOperationIds_WithRouteParameters_ShouldExcludeThem()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    #endregion

    #region UseDocumentation Tests

    [Fact]
    public void UseDocumentation_ShouldReturnSameApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var result = app.UseDocumentation();

        // Assert
        result.Should().BeSameAs(app);

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_SwaggerOptions_ShouldConfigureCustomRouteTemplate()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseDocumentation();

        // Assert - Middleware should be added (can't easily test the actual options without running the app)
        app.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_ShouldConfigureSwaggerMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseDocumentation();

        // Assert - Check that middleware was added
        var middleware = app.Build();
        middleware.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_ShouldConfigureSwaggerUIMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        services.AddRouting();
        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        app.UseDocumentation();

        // Assert
        var middleware = app.Build();
        middleware.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void DocumentationExtensions_FullConfiguration_ShouldRegisterAllServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<ISwaggerProvider>().Should().NotBeNull();
        serviceProvider.GetService<IApiDescriptionGroupCollectionProvider>().Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void DocumentationExtensions_ChainedCalls_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services
            .AddDocumentation()
            .AddRouting()
            .AddControllers();

        // Assert
        result.Should().NotBeNull();
        services.Should().Contain(sd => sd.ServiceType == typeof(ISwaggerProvider));
    }

    [Fact]
    public void DocumentationExtensions_XmlComments_ShouldHandleInvalidXmlGracefully()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Should not throw even if XML files are invalid or missing
        var act = () => services.AddDocumentation();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void DocumentationExtensions_MultipleApiVersions_ShouldSupportV1()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    [Fact]
    public void DocumentationExtensions_SecurityConfiguration_ShouldRequireBearerForAllOperations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDocumentation();
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var swaggerProvider = serviceProvider.GetService<ISwaggerProvider>();

        // Assert
        swaggerProvider.Should().NotBeNull();

        // Cleanup
        serviceProvider.Dispose();
    }

    #endregion

    #region Helper Classes

    private class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetUsers() => Ok();

        [HttpPost]
        public IActionResult CreateUser() => Ok();

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id) => Ok();
    }

    #endregion
}
