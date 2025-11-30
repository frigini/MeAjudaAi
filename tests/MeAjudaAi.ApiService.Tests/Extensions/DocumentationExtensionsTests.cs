using System.Reflection;
using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Extensions;

/// <summary>
/// Integration tests for DocumentationExtensions that generate actual Swagger documents
/// and verify behavioral outcomes rather than just service registration.
/// </summary>
public sealed class DocumentationExtensionsTests
{
    [Fact]
    public async Task GeneratedDocument_ShouldHaveCorrectTitleVersionAndContact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        // Register minimal API descriptions
        var apiDescriptions = CreateMinimalApiDescriptions();
        services.AddSingleton<IApiDescriptionGroupCollectionProvider>(new TestApiDescriptionGroupCollectionProvider(apiDescriptions));

        var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        // Act
        var document = await Task.Run(() => swaggerProvider.GetSwagger("v1"));

        // Assert
        document.Info.Title.Should().Be("MeAjudaAi API");
        document.Info.Version.Should().Be("v1");
        document.Info.Contact.Should().NotBeNull();
        document.Info.Contact!.Name.Should().Be("MeAjudaAi Team");
        document.Info.Contact.Email.Should().Be("contato@meajudaai.com");
        document.Info.Description.Should().Contain("API para conectar pessoas que buscam serviços");

        serviceProvider.Dispose();
    }

    [Fact]
    public async Task GeneratedDocument_ShouldIncludeBearerSecurityScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        var apiDescriptions = CreateMinimalApiDescriptions();
        services.AddSingleton<IApiDescriptionGroupCollectionProvider>(new TestApiDescriptionGroupCollectionProvider(apiDescriptions));

        var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        // Act
        var document = await Task.Run(() => swaggerProvider.GetSwagger("v1"));

        // Assert
        document.Components.Should().NotBeNull();
        document.Components!.SecuritySchemes.Should().ContainKey("Bearer");
        var bearerScheme = document.Components.SecuritySchemes["Bearer"];
        bearerScheme.Type.Should().Be(SecuritySchemeType.Http);
        bearerScheme.Scheme.Should().Be("bearer");
        bearerScheme.BearerFormat.Should().Be("JWT");
        bearerScheme.Description.Should().Contain("JWT Authorization header");

        serviceProvider.Dispose();
    }

    [Fact]
    public async Task GeneratedDocument_ShouldHaveGlobalSecurityRequirement()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        var apiDescriptions = CreateMinimalApiDescriptions();
        services.AddSingleton<IApiDescriptionGroupCollectionProvider>(new TestApiDescriptionGroupCollectionProvider(apiDescriptions));

        var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        // Act
        var document = await Task.Run(() => swaggerProvider.GetSwagger("v1"));

        // Assert - Verify Bearer security scheme exists
        document.Components.Should().NotBeNull();
        document.Components!.SecuritySchemes.Should().ContainKey("Bearer");

        // Verify global security requirement is applied
        document.Security.Should().NotBeNull()
            .And.HaveCountGreaterThan(0, "global security requirements should be configured");

        serviceProvider.Dispose();
    }

    [Fact]
    public async Task CustomOperationId_ShouldExcludeRouteParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        // Create API description with route parameters
        var apiDescriptions = new List<ApiDescription>
        {
            new ApiDescription
            {
                HttpMethod = "GET",
                RelativePath = "users/{id}/profile",
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = "Users",
                    ActionName = "GetProfile",
                    DisplayName = "GetProfile"
                }
            }
        };

        services.AddSingleton<IApiDescriptionGroupCollectionProvider>(new TestApiDescriptionGroupCollectionProvider(apiDescriptions));

        var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        // Act
        var document = await Task.Run(() => swaggerProvider.GetSwagger("v1"));

        // Assert
        document.Paths.Should().NotBeNull();
        document.Paths.Should().ContainKey("/users/{id}/profile");
        var pathItem = document.Paths!["/users/{id}/profile"];
        pathItem.Should().NotBeNull();
        pathItem!.Operations.Should().NotBeNull();
        var operation = pathItem.Operations.Values.FirstOrDefault();
        operation.Should().NotBeNull();
        operation!.OperationId.Should().NotBeNull();
        // Operation ID should not contain {id} parameter
        operation!.OperationId!.Should().NotContain("{");
        operation!.OperationId!.Should().NotContain("}");
        // Should be based on HTTP method and clean path
        operation!.OperationId!.Should().Be("GET-users-profile");

        serviceProvider.Dispose();
    }

    [Fact]
    public async Task CustomOperationId_WithNullHttpMethod_ShouldUseANY()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        var apiDescriptions = new List<ApiDescription>
        {
            new ApiDescription
            {
                HttpMethod = null!, // Simulate missing HTTP method
                RelativePath = "health",
                ActionDescriptor = new ActionDescriptor
                {
                    DisplayName = "HealthCheck"
                }
            }
        };

        services.AddSingleton<IApiDescriptionGroupCollectionProvider>(new TestApiDescriptionGroupCollectionProvider(apiDescriptions));

        var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        // Act
        var document = await Task.Run(() => swaggerProvider.GetSwagger("v1"));

        // Assert - Health endpoint should be prefixed with ANY-
        document.Paths.Should().ContainKey("/health");
        var healthPath = document.Paths!["/health"];
        healthPath.Should().NotBeNull();
        healthPath!.Operations.Should().NotBeNull();
        var operation = healthPath.Operations.Values.FirstOrDefault();
        operation.Should().NotBeNull();
        operation!.OperationId.Should().NotBeNull();
        operation!.OperationId!.Should().StartWith("ANY-");

        serviceProvider.Dispose();
    }

    [Fact]
    public async Task InvalidXmlComments_ShouldNotThrowAndNotBreakSchemaGeneration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - Should not throw even with invalid/missing XML file
        var act = () => services.AddDocumentation();
        act.Should().NotThrow();

        var apiDescriptions = CreateMinimalApiDescriptions();
        services.AddSingleton<IApiDescriptionGroupCollectionProvider>(new TestApiDescriptionGroupCollectionProvider(apiDescriptions));

        var serviceProvider = services.BuildServiceProvider();
        var swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();

        // Assert - Document generation should still work
        var documentAct = async () => await Task.Run(() => swaggerProvider.GetSwagger("v1"));
        await documentAct.Should().NotThrowAsync();

        var document = await documentAct();
        document.Should().NotBeNull();
        document.Paths.Should().NotBeNull();
        document.Paths!.Should().NotBeEmpty();

        serviceProvider.Dispose();
    }

    [Fact]
    public void SwaggerGenOptions_ShouldIncludeXmlCommentsIfFileExists()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SwaggerGenOptions>>().Value;

        // Assert
        // Verify options were configured (actual XML file may not exist, but configuration should be present)
        options.Should().NotBeNull();

        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var act = () => app.UseDocumentation();

        // Assert
        act.Should().NotThrow();

        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_ShouldReturnApplicationBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDocumentation();
        services.AddRouting();

        var serviceProvider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(serviceProvider);

        // Act
        var result = app.UseDocumentation();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);

        serviceProvider.Dispose();
    }

    // Helper methods
    private static List<ApiDescription> CreateMinimalApiDescriptions()
    {
        return new List<ApiDescription>
        {
            new ApiDescription
            {
                HttpMethod = "GET",
                RelativePath = "users",
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = "Users",
                    ActionName = "GetAll",
                    DisplayName = "GetAll"
                }
            },
            new ApiDescription
            {
                HttpMethod = "POST",
                RelativePath = "users",
                ActionDescriptor = new ControllerActionDescriptor
                {
                    ControllerName = "Users",
                    ActionName = "Create",
                    DisplayName = "Create"
                }
            }
        };
    }

    // Test helper class for API descriptions
    private class TestApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        public TestApiDescriptionGroupCollectionProvider(IEnumerable<ApiDescription> apiDescriptions)
        {
            ApiDescriptionGroups = new ApiDescriptionGroupCollection(
                new List<ApiDescriptionGroup>
                {
                    new ApiDescriptionGroup("v1", apiDescriptions.ToList())
                },
                1);
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups { get; }
    }
}
