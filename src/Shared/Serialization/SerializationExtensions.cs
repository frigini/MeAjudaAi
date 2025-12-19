using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Serialization;

/// <summary>
/// Extension methods para configuração de Serialization (JSON)
/// </summary>
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

        return services;
    }
}
