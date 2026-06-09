using MeAjudaAi.Shared.Serialization;
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
        services.AddKeyedScoped<ISerializer>("Default", (sp, key) => new SystemTextJsonSerializer(SerializationDefaults.Default));
        services.AddKeyedScoped<ISerializer>("Api", (sp, key) => new SystemTextJsonSerializer(SerializationDefaults.Api));

        return services;
    }
}
