using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Serialization.Converters;

/// <summary>
/// Conversor de enum estrito que rejeita valores indefinidos/inválidos durante a desserialização.
/// Diferente do JsonStringEnumConverter, este lança exceção ao receber valores numéricos inválidos.
/// </summary>
public class StrictEnumConverter : JsonConverterFactory
{
    private readonly JsonNamingPolicy? _namingPolicy;
    private readonly bool _allowIntegerValues;

    public StrictEnumConverter() : this(null, allowIntegerValues: true)
    {
    }

    public StrictEnumConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
    {
        _namingPolicy = namingPolicy;
        _allowIntegerValues = allowIntegerValues;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(StrictEnumConverterInner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType, _namingPolicy, _allowIntegerValues)!;
    }

    private class StrictEnumConverterInner<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
    {
        private readonly JsonNamingPolicy? _namingPolicy;
        private readonly bool _allowIntegerValues;

        public StrictEnumConverterInner(JsonNamingPolicy? namingPolicy, bool allowIntegerValues)
        {
            _namingPolicy = namingPolicy;
            _allowIntegerValues = allowIntegerValues;
        }

        public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var enumString = reader.GetString();
                if (string.IsNullOrWhiteSpace(enumString))
                {
                    throw new JsonException($"String vazia não é um valor válido para {typeof(TEnum).Name}");
                }

                // Try parse with naming policy
                if (Enum.TryParse<TEnum>(enumString, ignoreCase: true, out var result))
                {
                    // Validate that the parsed value is actually defined in the enum
                    if (Enum.IsDefined(typeof(TEnum), result))
                    {
                        return result;
                    }
                }

                throw new JsonException($"'{enumString}' não é um valor válido para {typeof(TEnum).Name}. Valores válidos: {string.Join(", ", Enum.GetNames<TEnum>())}");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (!_allowIntegerValues)
                {
                    throw new JsonException($"Valores inteiros não são permitidos para {typeof(TEnum).Name}. Use valores em string: {string.Join(", ", Enum.GetNames<TEnum>())}");
                }

                // Use GetInt64 to support enums with long/ulong underlying types
                var enumValue = reader.GetInt64();
                var result = (TEnum)Enum.ToObject(typeof(TEnum), enumValue);

                // Critical: Validate that the numeric value is actually defined in the enum
                // Note: Enum.IsDefined returns false for valid [Flags] combinations
                if (!Enum.IsDefined(typeof(TEnum), result))
                {
                    throw new JsonException($"{enumValue} não é um valor válido para {typeof(TEnum).Name}. Valores válidos: {string.Join(", ", Enum.GetValues<TEnum>())}");
                }

                return result;
            }

            throw new JsonException($"Tipo de token inesperado {reader.TokenType} para {typeof(TEnum).Name}");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            var enumString = value.ToString();
            
            if (_namingPolicy != null)
            {
                enumString = _namingPolicy.ConvertName(enumString);
            }

            writer.WriteStringValue(enumString);
        }
    }
}
