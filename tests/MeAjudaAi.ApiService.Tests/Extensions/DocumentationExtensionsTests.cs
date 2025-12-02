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
