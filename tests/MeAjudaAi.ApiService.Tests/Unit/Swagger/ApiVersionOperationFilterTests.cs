using FluentAssertions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Tests.Unit.Swagger;

/// <summary>
/// Unit tests for <see cref="ApiVersionOperationFilter"/> to verify API versioning behavior in Swagger documentation.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class ApiVersionOperationFilterTests
{
    private readonly ApiVersionOperationFilter _filter = new();

    [Fact]
    public void Apply_WithVersionParameter_ShouldRemoveVersionParameter()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<IOpenApiParameter>
            {
                new OpenApiParameter { Name = "version", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "id", In = ParameterLocation.Path }
            }
        };
        var context = CreateOperationFilterContext();

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().HaveCount(1);
        operation.Parameters.Should().NotContain(p => p.Name == "version");
        operation.Parameters.Should().Contain(p => p.Name == "id");
    }

    [Fact]
    public void Apply_WithoutVersionParameter_ShouldNotModifyParameters()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<IOpenApiParameter>
            {
                new OpenApiParameter { Name = "id", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "query", In = ParameterLocation.Query }
            }
        };
        var context = CreateOperationFilterContext();

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().HaveCount(2);
        operation.Parameters.Should().Contain(p => p.Name == "id");
        operation.Parameters.Should().Contain(p => p.Name == "query");
    }

    [Fact]
    public void Apply_WithNullParameters_ShouldNotThrow()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = null };
        var context = CreateOperationFilterContext();

        // Act & Assert
        var act = () => _filter.Apply(operation, context);
        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_WithEmptyParameters_ShouldNotThrow()
    {
        // Arrange
        var operation = new OpenApiOperation { Parameters = new List<IOpenApiParameter>() };
        var context = CreateOperationFilterContext();

        // Act & Assert
        var act = () => _filter.Apply(operation, context);
        act.Should().NotThrow();
        operation.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Apply_WithMultipleVersionParameters_ShouldRemoveOnlyFirst()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<IOpenApiParameter>
            {
                new OpenApiParameter { Name = "version", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "id", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "version", In = ParameterLocation.Query }
            }
        };
        var context = CreateOperationFilterContext();

        // Act
        _filter.Apply(operation, context);

        // Assert - FirstOrDefault removes only the first match
        operation.Parameters.Should().HaveCount(2);
        operation.Parameters.Should().Contain(p => p.Name == "id");
        operation.Parameters.Should().Contain(p => p.Name == "version" && p.In == ParameterLocation.Query);
    }

    [Fact]
    public void Apply_WithCaseSensitiveVersionName_ShouldOnlyRemoveExactMatch()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<IOpenApiParameter>
            {
                new OpenApiParameter { Name = "Version", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "version", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "api-version", In = ParameterLocation.Query }
            }
        };
        var context = CreateOperationFilterContext();

        // Act
        _filter.Apply(operation, context);

        // Assert - Should only remove lowercase "version"
        operation.Parameters.Should().HaveCount(2);
        operation.Parameters.Should().Contain(p => p.Name == "Version");
        operation.Parameters.Should().Contain(p => p.Name == "api-version");
    }

    private static OperationFilterContext CreateOperationFilterContext()
    {
        var methodInfo = typeof(ApiVersionOperationFilterTests).GetMethod(nameof(CreateOperationFilterContext));
        var apiDescription = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription();
        var schemaRepository = new SchemaRepository();
        var document = new OpenApiDocument();

        return new OperationFilterContext(
            apiDescription,
            null!,
            schemaRepository,
            document,
            methodInfo!
        );
    }
}
