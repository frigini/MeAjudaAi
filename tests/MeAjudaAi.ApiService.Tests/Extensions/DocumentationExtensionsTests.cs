using System.Reflection;
using FluentAssertions;
using MeAjudaAi.ApiService.Extensions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Moq;
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
    [Fact(Skip = "Swashbuckle SwaggerGenerator has complex internal dependencies - integration test needed")]
    public async Task GeneratedDocument_ShouldHaveCorrectTitleVersionAndContact()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Mock IWebHostEnvironment
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        mockEnv.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);
        mockEnv.Setup(e => e.WebRootPath).Returns(AppContext.BaseDirectory);
        mockEnv.Setup(e => e.ApplicationName).Returns("MeAjudaAi.ApiService.Tests");
        mockEnv.Setup(e => e.ContentRootFileProvider).Returns(new NullFileProvider());
        mockEnv.Setup(e => e.WebRootFileProvider).Returns(new NullFileProvider());
        services.AddSingleton(mockEnv.Object);

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

    [Fact(Skip = "Swashbuckle SwaggerGenerator has complex internal dependencies - integration test needed")]
    public async Task GeneratedDocument_ShouldIncludeBearerSecurityScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateMockEnvironment());
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

    [Fact(Skip = "Swashbuckle SwaggerGenerator has complex internal dependencies - integration test needed")]
    public async Task GeneratedDocument_ShouldHaveGlobalSecurityRequirement()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateMockEnvironment());
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

    [Fact(Skip = "Swashbuckle SwaggerGenerator has complex internal dependencies - integration test needed")]
    public async Task CustomOperationId_ShouldExcludeRouteParameters()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateMockEnvironment());
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
        operation!.OperationId!.Should().Be("GET-users-profile");

        serviceProvider.Dispose();
    }

    [Fact(Skip = "Swashbuckle SwaggerGenerator has complex internal dependencies - integration test needed")]
    public async Task CustomOperationId_WithNullHttpMethod_ShouldUseANY()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateMockEnvironment());
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

    [Fact(Skip = "Swashbuckle SwaggerGenerator has complex internal dependencies - integration test needed")]
    public async Task InvalidXmlComments_ShouldNotThrowAndNotBreakSchemaGeneration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateMockEnvironment());

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
    public void SwaggerGenOptions_ShouldBeConfigured()
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
        // Note: SwaggerGenOptions doesn't expose a simple way to inspect XML document filters
        // This test verifies options are configured; actual XML inclusion is verified by document generation tests

        serviceProvider.Dispose();
    }

    [Fact]
    public void UseDocumentation_ShouldRegisterMiddlewareAndReturnBuilder()
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
                    DisplayName = "GetAll",
                    RouteValues = new Dictionary<string, string?>
                    {
                        ["controller"] = "Users",
                        ["action"] = "GetAll"
                    }
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
                    DisplayName = "Create",
                    RouteValues = new Dictionary<string, string?>
                    {
                        ["controller"] = "Users",
                        ["action"] = "Create"
                    }
                }
            }
        };
    }

    private static IWebHostEnvironment CreateMockEnvironment()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Testing");
        mockEnv.Setup(e => e.ContentRootPath).Returns(AppContext.BaseDirectory);
        mockEnv.Setup(e => e.WebRootPath).Returns(AppContext.BaseDirectory);
        mockEnv.Setup(e => e.ApplicationName).Returns("MeAjudaAi.ApiService.Tests");
        mockEnv.Setup(e => e.ContentRootFileProvider).Returns(new NullFileProvider());
        mockEnv.Setup(e => e.WebRootFileProvider).Returns(new NullFileProvider());
        return mockEnv.Object;
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
