using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.ApiService.Filters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace MeAjudaAi.ApiService.Tests.Filters;

public class ModuleTagsDocumentFilterTests
{
    private readonly ModuleTagsDocumentFilter _filter;

    public ModuleTagsDocumentFilterTests()
    {
        _filter = new ModuleTagsDocumentFilter();
    }

    [Fact]
    public void Apply_ShouldNotThrowWithValidDocument()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        var act = () => _filter.Apply(swaggerDoc, context);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Apply_ShouldInitializeTags()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Tags.Should().NotBeNull();
        swaggerDoc.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public void Apply_ShouldIncludeUsersTag()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Tags.Should().Contain(t => t.Name == "Users");
    }

    [Fact]
    public void Apply_ShouldIncludeHealthTag()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Tags.Should().Contain(t => t.Name == "Health");
    }

    [Fact]
    public void Apply_UsersTag_ShouldHaveDescription()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var usersTag = swaggerDoc.Tags!.First(t => t.Name == "Users");
        usersTag.Description.Should().Be("Gerenciamento de usuários, perfis e autenticação");
    }

    [Fact]
    public void Apply_HealthTag_ShouldHaveDescription()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var healthTag = swaggerDoc.Tags!.First(t => t.Name == "Health");
        healthTag.Description.Should().Be("Monitoramento e health checks dos serviços");
    }

    [Fact]
    public void Apply_ShouldMaintainTagOrder()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var tagNames = swaggerDoc.Tags!.Select(t => t.Name).ToList();
        var usersIndex = tagNames.IndexOf("Users");
        var healthIndex = tagNames.IndexOf("Health");
        usersIndex.Should().BeGreaterThanOrEqualTo(0);
        healthIndex.Should().BeGreaterThan(usersIndex);
    }

    [Fact]
    public void Apply_ShouldHandleNullPaths()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        var act = () => _filter.Apply(swaggerDoc, context);

        // Assert
        act.Should().NotThrow();
        swaggerDoc.Tags.Should().NotBeNull();
    }

    [Fact]
    public void Apply_ShouldHandleEmptyPaths()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument { Paths = new OpenApiPaths() };
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Tags.Should().NotBeNull();
        swaggerDoc.Tags.Should().Contain(t => t.Name == "Users");
    }

    [Fact]
    public void Apply_ShouldInitializeComponents()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components.Should().NotBeNull();
    }

    [Fact]
    public void Apply_ShouldInitializeExamples()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components!.Examples.Should().NotBeNull();
        swaggerDoc.Components.Examples.Should().NotBeEmpty();
    }

    [Fact]
    public void Apply_ShouldAddErrorResponseExample()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components!.Examples!.Should().ContainKey("ErrorResponse");
    }

    [Fact]
    public void Apply_ErrorResponseExample_ShouldHaveSummary()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var errorExample = swaggerDoc.Components!.Examples!["ErrorResponse"];
        errorExample.Summary.Should().Be("Resposta de Erro Padrão");
    }

    [Fact]
    public void Apply_ErrorResponseExample_ShouldHaveDescription()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var errorExample = swaggerDoc.Components!.Examples!["ErrorResponse"];
        errorExample.Description.Should().Be("Formato padrão das respostas de erro da API");
    }

    [Fact]
    public void Apply_ErrorResponseExample_ShouldHaveValue()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var errorExample = swaggerDoc.Components!.Examples!["ErrorResponse"];
        errorExample.Value.Should().NotBeNull();
    }

    [Fact]
    public void Apply_ShouldAddSuccessResponseExample()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components!.Examples!.Should().ContainKey("SuccessResponse");
    }

    [Fact]
    public void Apply_SuccessResponseExample_ShouldHaveSummary()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var successExample = swaggerDoc.Components!.Examples!["SuccessResponse"];
        successExample.Summary.Should().Be("Resposta de Sucesso Padrão");
    }

    [Fact]
    public void Apply_SuccessResponseExample_ShouldHaveDescription()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var successExample = swaggerDoc.Components!.Examples!["SuccessResponse"];
        successExample.Description.Should().Be("Formato padrão das respostas de sucesso da API");
    }

    [Fact]
    public void Apply_SuccessResponseExample_ShouldHaveValue()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var successExample = swaggerDoc.Components!.Examples!["SuccessResponse"];
        successExample.Value.Should().NotBeNull();
    }

    [Fact]
    public void Apply_ShouldInitializeSchemas()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components!.Schemas.Should().NotBeNull();
        swaggerDoc.Components.Schemas.Should().NotBeEmpty();
    }

    [Fact]
    public void Apply_ShouldAddPaginationMetadataSchema()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components!.Schemas!.Should().ContainKey("PaginationMetadata");
    }

    [Fact]
    public void Apply_PaginationMetadataSchema_ShouldHaveObjectType()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var schema = swaggerDoc.Components!.Schemas!["PaginationMetadata"];
        schema.Type.Should().Be(JsonSchemaType.Object);
    }

    [Fact]
    public void Apply_PaginationMetadataSchema_ShouldHaveDescription()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var schema = swaggerDoc.Components!.Schemas!["PaginationMetadata"];
        schema.Description.Should().Be("Metadados de paginação para listagens");
    }

    [Theory]
    [InlineData("page")]
    [InlineData("pageSize")]
    [InlineData("totalItems")]
    [InlineData("totalPages")]
    [InlineData("hasNextPage")]
    [InlineData("hasPreviousPage")]
    public void Apply_PaginationMetadataSchema_ShouldHaveRequiredProperty(string propertyName)
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument();
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        var schema = swaggerDoc.Components!.Schemas!["PaginationMetadata"];
        schema.Properties.Should().ContainKey(propertyName);
        schema.Required.Should().Contain(propertyName);
    }

    [Fact]
    public void Apply_ShouldPreserveExistingExamples()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Examples = new Dictionary<string, IOpenApiExample>
                {
                    ["ExistingExample"] = new OpenApiExample { Summary = "Existing" }
                }
            }
        };
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components.Examples.Should().ContainKey("ExistingExample");
        swaggerDoc.Components.Examples.Should().ContainKey("ErrorResponse");
        swaggerDoc.Components.Examples.Should().ContainKey("SuccessResponse");
    }

    [Fact]
    public void Apply_ShouldPreserveExistingSchemas()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["ExistingSchema"] = new OpenApiSchema { Type = JsonSchemaType.Object }
                }
            }
        };
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components.Schemas.Should().ContainKey("ExistingSchema");
        swaggerDoc.Components.Schemas.Should().ContainKey("PaginationMetadata");
    }

    [Fact]
    public void Apply_WithNullComponents_ShouldInitializeComponents()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument { Components = null };
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components.Should().NotBeNull();
    }

    [Fact]
    public void Apply_WithNullExamples_ShouldInitializeExamples()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument
        {
            Components = new OpenApiComponents { Examples = null }
        };
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components.Examples.Should().NotBeNull();
        swaggerDoc.Components.Examples.Should().ContainKey("ErrorResponse");
    }

    [Fact]
    public void Apply_WithNullSchemas_ShouldInitializeSchemas()
    {
        // Arrange
        var swaggerDoc = new OpenApiDocument
        {
            Components = new OpenApiComponents { Schemas = null }
        };
        var context = CreateDocumentFilterContext();

        // Act
        _filter.Apply(swaggerDoc, context);

        // Assert
        swaggerDoc.Components.Schemas.Should().NotBeNull();
        swaggerDoc.Components.Schemas.Should().ContainKey("PaginationMetadata");
    }

    // Helper methods
    private static DocumentFilterContext CreateDocumentFilterContext()
    {
        var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), new JsonSerializerDataContractResolver(new JsonSerializerOptions()));
        return new DocumentFilterContext(
            new List<ApiDescription>(),
            schemaGenerator,
            new SchemaRepository());
    }
}
