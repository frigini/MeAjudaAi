using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Serialization;

internal static class Extensions
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