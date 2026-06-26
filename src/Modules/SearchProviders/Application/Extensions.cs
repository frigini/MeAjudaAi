using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.ModuleApi;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MeAjudaAi.Modules.SearchProviders.Application;

/// <summary>
/// Métodos de extensão para registrar serviços da camada de Application do SearchProviders.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
    /// <summary>
    /// Registra serviços da camada de Application do SearchProviders.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar query handlers
        services.AddScoped<IQueryHandler<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>, Handlers.SearchProvidersQueryHandler>();

        // Registrar validadores FluentValidation
        services.AddModuleValidators(Assembly.GetExecutingAssembly());

        // Registrar API do módulo
        services.AddScoped<ISearchProvidersModuleApi, SearchProvidersModuleApi>();

        return services;
    }
}
