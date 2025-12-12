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

    private static double GetDecimalExample(string propertyName)
    {
        /*
        return propertyName switch
        {
            var name when name.Contains("price") || name.Contains("preco") => 99.99,
            var name when name.Contains("rate") || name.Contains("taxa") => 4.5,
            var name when name.Contains("percentage") || name.Contains("porcentagem") => 15.0,
            _ => 1.0
        };
        */
        return 1.0;
    }
}

#pragma warning restore IDE0051, IDE0060
