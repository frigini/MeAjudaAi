using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Serialization.Converters;

/// <summary>
/// Strict enum converter that rejects undefined/invalid enum values during deserialization.
/// Unlike JsonStringEnumConverter, this throws when receiving invalid numeric values.
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
                    throw new JsonException($"Empty string is not a valid {typeof(TEnum).Name} value");
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

                throw new JsonException($"'{enumString}' is not a valid {typeof(TEnum).Name} value. Valid values: {string.Join(", ", Enum.GetNames<TEnum>())}");
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                if (!_allowIntegerValues)
                {
                    throw new JsonException($"Integer values are not allowed for {typeof(TEnum).Name}. Use string values: {string.Join(", ", Enum.GetNames<TEnum>())}");
                }

                var enumValue = reader.GetInt32();
                var result = (TEnum)Enum.ToObject(typeof(TEnum), enumValue);

                // Critical: Validate that the numeric value is actually defined in the enum
                if (!Enum.IsDefined(typeof(TEnum), result))
                {
                    throw new JsonException($"{enumValue} is not a valid {typeof(TEnum).Name} value. Valid values: {string.Join(", ", Enum.GetValues<TEnum>())}");
                }

                return result;
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} for {typeof(TEnum).Name}");
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
