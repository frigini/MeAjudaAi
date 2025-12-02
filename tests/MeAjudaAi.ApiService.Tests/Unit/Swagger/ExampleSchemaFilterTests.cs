using FluentAssertions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Tests.Unit.Swagger;

[Trait("Category", "Unit")]
[Trait("Layer", "ApiService")]
public class ExampleSchemaFilterTests
{
    private readonly ExampleSchemaFilter _filter = new();

    [Fact]
    public void Apply_ShouldThrowNotImplementedException_DueToSwashbuckleMigration()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(string));

        // Act & Assert
        var act = () => _filter.Apply(schema, context);
        act.Should().Throw<NotImplementedException>()
            .WithMessage("*Swashbuckle 10.x*reflexão*Example*");
    }

    [Fact]
    public void Apply_WithNullSchema_ShouldThrowNotImplementedException()
    {
        // Arrange
        var context = CreateSchemaFilterContext(typeof(string));

        // Act & Assert
        var act = () => _filter.Apply(null!, context);
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Apply_WithClassType_ShouldThrowNotImplementedException()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(TestClass));

        // Act & Assert
        var act = () => _filter.Apply(schema, context);
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Apply_WithEnumType_ShouldThrowNotImplementedException()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(TestEnum));

        // Act & Assert
        var act = () => _filter.Apply(schema, context);
        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Apply_WithPrimitiveType_ShouldThrowNotImplementedException()
    {
        // Arrange
        var schema = new OpenApiSchema();
        var context = CreateSchemaFilterContext(typeof(int));

        // Act & Assert
        var act = () => _filter.Apply(schema, context);
        act.Should().Throw<NotImplementedException>();
    }

    private static SchemaFilterContext CreateSchemaFilterContext(Type type)
    {
        return new SchemaFilterContext(
            type: type,
            schemaGenerator: null!,
            schemaRepository: new SchemaRepository()
        );
    }

    // Test helper types
    private class TestClass
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}
