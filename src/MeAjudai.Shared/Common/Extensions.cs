using FluentValidation;
using FluentValidation.Results;
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

    public static async Task<ValidationResult> ValidateAsync<T>(
        this IServiceProvider serviceProvider,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var validator = serviceProvider.GetService<IValidator<T>>();
        return validator != null
            ? await validator.ValidateAsync(instance, cancellationToken)
            : new ValidationResult();
    }
}