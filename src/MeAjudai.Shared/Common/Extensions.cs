using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MeAjudaAi.Shared.Common;

public static class Extensions
{
    public static IServiceCollection AddStructuredLogging(
        this IServiceCollection services)
    {
        services.AddSerilog();
        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        return services;
    }
}