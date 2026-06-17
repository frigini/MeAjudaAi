using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Shared.Serialization;

/// <summary>
/// Extension methods para configuração de Serialization (JSON)
/// </summary>
[ExcludeFromCodeCoverage]
public static class SerializationExtensions
{
    public static IServiceCollection AddCustomSerialization(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = SerializationDefaults.Default.PropertyNamingPolicy;
            options.SerializerOptions.PropertyNameCaseInsensitive = SerializationDefaults.Default.PropertyNameCaseInsensitive;

            foreach (var converter in SerializationDefaults.Default.Converters)
            {
                options.SerializerOptions.Converters.Add(converter);
            }
        });

        // Registro de ISerializer como Keyed Services
        services.AddKeyedScoped<ISerializer>(SerializationKeys.Default, (_, _) => new SystemTextJsonSerializer(SerializationDefaults.Default));
        services.AddKeyedScoped<ISerializer>(SerializationKeys.Api, (_, _) => new SystemTextJsonSerializer(SerializationDefaults.Api));
        services.AddKeyedScoped<ISerializer>(SerializationKeys.Logging, (_, _) => new SystemTextJsonSerializer(SerializationDefaults.Logging));

        return services;
    }
}
