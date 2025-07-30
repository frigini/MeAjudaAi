using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MeAjudai.Shared.Common;

public static class Extensions
{
    public static IServiceCollection AddLogging(
        this IServiceCollection services)
    {
        services.AddSerilog();
        return services;
    }
}