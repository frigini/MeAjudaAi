using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace MeAjudaAi.ApiService.Filters;

/// <summary>
/// Filtro para adicionar exemplos automáticos aos schemas baseado em atributos
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
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
    }

    private void AddExamplesFromProperties(OpenApiSchema schema, Type type)
    {
        if (schema.Properties == null) return;

        var example = new OpenApiObject();
        var hasExamples = false;

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
            
            if (!schema.Properties.ContainsKey(propertyName)) continue;

            var exampleValue = GetPropertyExample(property);
            if (exampleValue != null)
            {
                example[propertyName] = exampleValue;
                hasExamples = true;
            }
        }

        if (hasExamples)
        {
            schema.Example = example;
        }
    }

    private IOpenApiAny? GetPropertyExample(PropertyInfo property)
    {
        // Verificar atributo DefaultValue
        var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
        if (defaultValueAttr != null)
        {
            return ConvertToOpenApiAny(defaultValueAttr.Value);
        }

        // Exemplos baseados no tipo e nome da propriedade
        var propertyName = property.Name.ToLowerInvariant();
        var propertyType = property.PropertyType;

        // Handle nullable types
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyType = Nullable.GetUnderlyingType(propertyType)!;
        }

        return propertyType.Name switch
        {
            nameof(String) => GetStringExample(propertyName),
            nameof(Guid) => new OpenApiString(Guid.NewGuid().ToString()),
            nameof(DateTime) => new OpenApiDateTime(DateTime.UtcNow),
            nameof(DateTimeOffset) => new OpenApiDateTime(DateTimeOffset.UtcNow),
            nameof(Int32) => new OpenApiInteger(GetIntegerExample(propertyName)),
            nameof(Int64) => new OpenApiLong(GetLongExample(propertyName)),
            nameof(Boolean) => new OpenApiBoolean(GetBooleanExample(propertyName)),
            nameof(Decimal) => new OpenApiDouble(GetDecimalExample(propertyName)),
            nameof(Double) => new OpenApiDouble(GetDoubleExample(propertyName)),
            _ => null
        };
    }

    private static IOpenApiAny GetStringExample(string propertyName)
    {
        return propertyName switch
        {
            var name when name.Contains("email") => new OpenApiString("usuario@example.com"),
            var name when name.Contains("phone") || name.Contains("telefone") => new OpenApiString("+55 11 99999-9999"),
            var name when name.Contains("name") || name.Contains("nome") => new OpenApiString("João Silva"),
            var name when name.Contains("username") => new OpenApiString("joao.silva"),
            var name when name.Contains("firstname") => new OpenApiString("João"),
            var name when name.Contains("lastname") => new OpenApiString("Silva"),
            var name when name.Contains("password") => new OpenApiString("MinhaSenh@123"),
            var name when name.Contains("description") || name.Contains("descricao") => new OpenApiString("Descrição do item"),
            var name when name.Contains("title") || name.Contains("titulo") => new OpenApiString("Título do Item"),
            var name when name.Contains("address") || name.Contains("endereco") => new OpenApiString("Rua das Flores, 123"),
            var name when name.Contains("city") || name.Contains("cidade") => new OpenApiString("São Paulo"),
            var name when name.Contains("state") || name.Contains("estado") => new OpenApiString("SP"),
            var name when name.Contains("zipcode") || name.Contains("cep") => new OpenApiString("01234-567"),
            var name when name.Contains("country") || name.Contains("pais") => new OpenApiString("Brasil"),
            _ => new OpenApiString("exemplo")
        };
    }

    private static int GetIntegerExample(string propertyName)
    {
        return propertyName switch
        {
            var name when name.Contains("age") || name.Contains("idade") => 30,
            var name when name.Contains("count") || name.Contains("quantity") => 10,
            var name when name.Contains("page") => 1,
            var name when name.Contains("size") || name.Contains("limit") => 20,
            var name when name.Contains("year") || name.Contains("ano") => DateTime.Now.Year,
            var name when name.Contains("month") || name.Contains("mes") => DateTime.Now.Month,
            var name when name.Contains("day") || name.Contains("dia") => DateTime.Now.Day,
            _ => 1
        };
    }

    private static long GetLongExample(string propertyName)
    {
        return propertyName switch
        {
            var name when name.Contains("timestamp") => DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            _ => 1L
        };
    }

    private static bool GetBooleanExample(string propertyName)
    {
        return propertyName switch
        {
            var name when name.Contains("active") || name.Contains("ativo") => true,
            var name when name.Contains("enabled") || name.Contains("habilitado") => true,
            var name when name.Contains("verified") || name.Contains("verificado") => true,
            var name when name.Contains("deleted") || name.Contains("excluido") => false,
            var name when name.Contains("disabled") || name.Contains("desabilitado") => false,
            _ => true
        };
    }

    private static double GetDecimalExample(string propertyName)
    {
        return propertyName switch
        {
            var name when name.Contains("price") || name.Contains("preco") => 99.99,
            var name when name.Contains("rate") || name.Contains("taxa") => 4.5,
            var name when name.Contains("percentage") || name.Contains("porcentagem") => 15.0,
            _ => 1.0
        };
    }

    private double GetDoubleExample(string propertyName)
    {
        return GetDecimalExample(propertyName);
    }

    private static void AddEnumExamples(OpenApiSchema schema, Type enumType)
    {
        var enumValues = Enum.GetValues(enumType);
        if (enumValues.Length > 0)
        {
            var firstValue = enumValues.GetValue(0);
            schema.Example = new OpenApiString(firstValue?.ToString());
        }
    }

    private static void AddDetailedDescription(OpenApiSchema schema, Type type)
    {
        var descriptionAttr = type.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr != null && string.IsNullOrEmpty(schema.Description))
        {
            schema.Description = descriptionAttr.Description;
        }
    }

    private static IOpenApiAny? ConvertToOpenApiAny(object? value)
    {
        return value switch
        {
            null => null,
            string s => new OpenApiString(s),
            int i => new OpenApiInteger(i),
            long l => new OpenApiLong(l),
            float f => new OpenApiFloat(f),
            double d => new OpenApiDouble(d),
            decimal dec => new OpenApiDouble((double)dec),
            bool b => new OpenApiBoolean(b),
            DateTime dt => new OpenApiDateTime(dt),
            DateTimeOffset dto => new OpenApiDateTime(dto),
            Guid g => new OpenApiString(g.ToString()),
            _ => new OpenApiString(value.ToString())
        };
    }
}