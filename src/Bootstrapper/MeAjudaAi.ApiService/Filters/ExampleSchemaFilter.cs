using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MeAjudaAi.ApiService.Filters;

/// <summary>
/// Filtro para adicionar exemplos automáticos aos schemas baseado em atributos
/// </summary>
public class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        // Swashbuckle v10 usa IOpenApiSchema; cast para OpenApiSchema para acessar propriedades
        if (schema is not OpenApiSchema openApiSchema) return;

        // Adicionar exemplos baseados em DefaultValueAttribute
        if (context.Type.IsClass && context.Type != typeof(string))
        {
            AddExamplesFromProperties(openApiSchema, context.Type);
        }

        // Adicionar exemplos para enums
        if (context.Type.IsEnum)
        {
            AddEnumExamples(openApiSchema, context.Type);
        }

        // Adicionar descrições mais detalhadas
        AddDetailedDescription(openApiSchema, context.Type);
    }

    private void AddExamplesFromProperties(OpenApiSchema schema, Type type)
    {
        if (schema.Properties == null) return;

        var example = new JsonObject();
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

    private JsonNode? GetPropertyExample(PropertyInfo property)
    {
        // Verificar atributo DefaultValue
        var defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
        if (defaultValueAttr != null)
        {
            return ConvertToJsonNode(defaultValueAttr.Value);
        }

        // Exemplos baseados no tipo e nome da propriedade
        var propertyName = property.Name.ToLowerInvariant();
        var propertyType = property.PropertyType;

        // Tratar tipos nullable
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            propertyType = Nullable.GetUnderlyingType(propertyType)!;
        }

        // Tratar tipos enum
        if (propertyType.IsEnum)
        {
            var enumNames = Enum.GetNames(propertyType);
            if (enumNames.Length > 0)
            {
                return JsonValue.Create(enumNames[0]);
            }
        }

        return propertyType.Name switch
        {
            nameof(String) => GetStringExample(propertyName),
            nameof(Guid) => JsonValue.Create("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            nameof(DateTime) => JsonValue.Create(new DateTime(2024, 01, 15, 10, 30, 00, DateTimeKind.Utc)),
            nameof(DateTimeOffset) => JsonValue.Create(new DateTimeOffset(2024, 01, 15, 10, 30, 00, TimeSpan.Zero)),
            nameof(Int32) => JsonValue.Create(GetIntegerExample(propertyName)),
            nameof(Int64) => JsonValue.Create(GetLongExample(propertyName)),
            nameof(Boolean) => JsonValue.Create(GetBooleanExample(propertyName)),
            nameof(Decimal) => JsonValue.Create(GetDecimalExample(propertyName)),
            nameof(Double) => JsonValue.Create(GetDoubleExample(propertyName)),
            _ => null
        };
    }

    private static JsonNode GetStringExample(string propertyName)
    {
        return propertyName switch
        {
            var name when name.Contains("email") => JsonValue.Create("usuario@example.com"),
            var name when name.Contains("phone") || name.Contains("telefone") => JsonValue.Create("+55 11 99999-9999"),
            var name when name.Contains("name") || name.Contains("nome") => JsonValue.Create("João Silva"),
            var name when name.Contains("username") => JsonValue.Create("joao.silva"),
            var name when name.Contains("firstname") => JsonValue.Create("João"),
            var name when name.Contains("lastname") => JsonValue.Create("Silva"),
            var name when name.Contains("password") => JsonValue.Create("MinhaSenh@123"),
            var name when name.Contains("description") || name.Contains("descricao") => JsonValue.Create("Descrição do item"),
            var name when name.Contains("title") || name.Contains("titulo") => JsonValue.Create("Título do Item"),
            var name when name.Contains("address") || name.Contains("endereco") => JsonValue.Create("Rua das Flores, 123"),
            var name when name.Contains("city") || name.Contains("cidade") => JsonValue.Create("São Paulo"),
            var name when name.Contains("state") || name.Contains("estado") => JsonValue.Create("SP"),
            var name when name.Contains("zipcode") || name.Contains("cep") => JsonValue.Create("01234-567"),
            var name when name.Contains("country") || name.Contains("pais") => JsonValue.Create("Brasil"),
            _ => JsonValue.Create("exemplo")
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

        // Use string representation for enums (JsonValue instead of OpenApiString in OpenApi 2.x)
        schema.Example = JsonValue.Create(firstValue.ToString()!);
    }

    private static void AddDetailedDescription(OpenApiSchema schema, Type type)
    {
        var descriptionAttr = type.GetCustomAttribute<DescriptionAttribute>();
        if (descriptionAttr != null && string.IsNullOrEmpty(schema.Description))
        {
            schema.Description = descriptionAttr.Description;
        }
    }

    private static JsonNode? ConvertToJsonNode(object? value)
    {
        return value switch
        {
            null => null,
            string s => JsonValue.Create(s),
            int i => JsonValue.Create(i),
            long l => JsonValue.Create(l),
            float f => JsonValue.Create(f),
            double d => JsonValue.Create(d),
            decimal dec => JsonValue.Create(dec),
            bool b => JsonValue.Create(b),
            DateTime dt => JsonValue.Create(dt),
            DateTimeOffset dto => JsonValue.Create(dto),
            Guid g => JsonValue.Create(g.ToString()),
            _ => null // Unsupported type; return null instead of ToString() to avoid unexpected JSON
        };
    }
}


