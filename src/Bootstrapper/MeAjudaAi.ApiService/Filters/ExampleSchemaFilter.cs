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
            var attrName = property.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>()?.Name;
            var candidates = new[]
            {
                attrName,
                property.Name,
                char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1)
            }.Where(n => !string.IsNullOrEmpty(n)).Cast<string>();

            var schemaKey = schema.Properties.Keys
                .FirstOrDefault(k => candidates.Any(c => string.Equals(k, c, StringComparison.Ordinal)));
            if (schemaKey == null) continue;

            var exampleValue = GetPropertyExample(property);
            if (exampleValue != null)
            {
                example[schemaKey] = exampleValue;
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
            nameof(Guid) => new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            nameof(DateTime) => new OpenApiDateTime(new DateTime(2024, 01, 15, 10, 30, 00, DateTimeKind.Utc)),
            nameof(DateTimeOffset) => new OpenApiDateTime(new DateTimeOffset(2024, 01, 15, 10, 30, 00, TimeSpan.Zero)),
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
        if (enumValues.Length == 0) return;

        var firstValue = enumValues.GetValue(0);
        if (firstValue == null) return;

        // Check if schema represents enum as integer or string
        var isIntegerEnum = schema.Type == "integer" || 
                           (schema.Enum?.Count > 0 && schema.Enum[0] is OpenApiInteger);

        if (isIntegerEnum)
        {
            // Try to convert enum to integer representation
            try
            {
                var underlyingType = Enum.GetUnderlyingType(enumType);
                var numericValue = Convert.ChangeType(firstValue, underlyingType);

                schema.Example = numericValue switch
                {
                    long l => new OpenApiLong(l),
                    int i => new OpenApiInteger(i),
                    short s => new OpenApiInteger(s),
                    byte b => new OpenApiInteger(b),
                    _ => new OpenApiInteger(Convert.ToInt32(numericValue))
                };
            }
            catch
            {
                // Fall back to string representation if numeric conversion fails
                schema.Example = new OpenApiString(firstValue.ToString());
            }
        }
        else
        {
            // Use string representation (existing behavior)
            schema.Example = new OpenApiString(firstValue.ToString());
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