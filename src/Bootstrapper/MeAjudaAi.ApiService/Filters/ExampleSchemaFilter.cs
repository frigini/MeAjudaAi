using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Filters;

// TODO: Migrar para Swashbuckle 10.x - IOpenApiSchema.Example é read-only
// SOLUÇÃO: Usar reflexão para acessar propriedade Example na implementação concreta
// Exemplo: schema.GetType().GetProperty("Example")?.SetValue(schema, exampleValue, null);
// Temporariamente desabilitado em DocumentationExtensions.cs

#pragma warning disable IDE0051, IDE0060 // Remove unused private members

/// <summary>
/// Filtro para adicionar exemplos automáticos aos schemas baseado em atributos
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // Swashbuckle 10.x: IOpenApiSchema.Example é read-only
        // SOLUÇÃO QUANDO REATIVAR: Usar reflexão para acessar implementação concreta
        // var exampleProp = schema.GetType().GetProperty("Example");
        // if (exampleProp?.CanWrite == true) exampleProp.SetValue(schema, value, null);
        throw new NotImplementedException("Precisa migração para Swashbuckle 10.x - usar reflexão para Example");

        /*
        // Adicionar exemplos baseados em DefaultValueAttribute
        if (context.Type.IsClass && context.Type != typeof(string))
        {
            AddExamplesFromProperties(schema, context.Type);
        }

        // Adicionar exemplos para enums
        if (context.Type.IsEnum)
        {
            AddEnumExamples(schema, context.Type);
        }

        // Adicionar descrições mais detalhadas
        AddDetailedDescription(schema, context.Type);
        */
    }
}

#pragma warning restore IDE0051, IDE0060
