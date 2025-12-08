using FluentAssertions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Unit.Filters;

public class ApiVersionOperationFilterTests
{
    private readonly ApiVersionOperationFilter _filter;

    public ApiVersionOperationFilterTests()
    {
        _filter = new ApiVersionOperationFilter();
    }

    [Fact]
    public void Apply_ShouldRemoveVersionParameter_WhenVersionParameterExists()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "version", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "id", In = ParameterLocation.Path }
            }
        };

        var context = CreateMockContext();

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().HaveCount(1);
        operation.Parameters.Should().NotContain(p => p.Name == "version");
        operation.Parameters.Should().Contain(p => p.Name == "id");
    }

    [Fact]
    public void Apply_ShouldDoNothing_WhenVersionParameterDoesNotExist()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "id", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "userId", In = ParameterLocation.Query }
            }
        };

        var context = CreateMockContext();

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().HaveCount(2);
        operation.Parameters.Should().Contain(p => p.Name == "id");
        operation.Parameters.Should().Contain(p => p.Name == "userId");
    }

    [Fact]
    public void Apply_ShouldHandleNullParameters_WhenParametersIsNull()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = null
        };

        var context = CreateMockContext();

        // Act
        var act = () => _filter.Apply(operation, context);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_ShouldHandleEmptyParameters_WhenParametersIsEmpty()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<OpenApiParameter>()
        };

        var context = CreateMockContext();

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Apply_ShouldOnlyRemoveVersionParameter_WhenMultipleParametersExist()
    {
        // Arrange
        var operation = new OpenApiOperation
        {
            Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "version", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "id", In = ParameterLocation.Path },
                new OpenApiParameter { Name = "userId", In = ParameterLocation.Query },
                new OpenApiParameter { Name = "filter", In = ParameterLocation.Query }
            }
        };

        var context = CreateMockContext();

        // Act
        _filter.Apply(operation, context);

        // Assert
        operation.Parameters.Should().HaveCount(3);
        operation.Parameters.Should().NotContain(p => p.Name == "version");
        operation.Parameters.Should().Contain(p => p.Name == "id");
        operation.Parameters.Should().Contain(p => p.Name == "userId");
        operation.Parameters.Should().Contain(p => p.Name == "filter");
    }

    private static OperationFilterContext CreateMockContext()
    {
        // Create a minimal mock context - the filter doesn't use any of these parameters
        var apiDescription = new Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription();
        var schemaRepository = new SchemaRepository();
        var methodInfo = typeof(ApiVersionOperationFilterTests).GetMethod(nameof(CreateMockContext))!;
        var schemaGenerator = new Mock<ISchemaGenerator>().Object;
        var openApiDocument = new OpenApiDocument();

        return new OperationFilterContext(
            apiDescription,
            schemaGenerator,
            schemaRepository,
            openApiDocument,
            methodInfo);
    }
}

