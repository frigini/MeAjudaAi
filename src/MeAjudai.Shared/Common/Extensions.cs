using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MeAjudaAi.Shared.Common;

public static class Extensions
{
    public static IServiceCollection AddLogging(
        this IServiceCollection services)
    {
        services.AddSerilog();
        return services;
    }
}