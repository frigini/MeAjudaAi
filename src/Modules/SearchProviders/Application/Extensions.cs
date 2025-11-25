using FluentValidation;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.ModuleApi;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.SearchProviders.Application;

/// <summary>
/// Métodos de extensão para registrar serviços da camada de Application do SearchProviders.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra serviços da camada de Application do SearchProviders.
    /// </summary>
    public static IServiceCollection AddSearchProvidersApplication(this IServiceCollection services)
    {
        // Registrar query handlers
        services.AddScoped<IQueryHandler<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>, Handlers.SearchProvidersQueryHandler>();

        // Registrar validadores do FluentValidation
        services.AddValidatorsFromAssembly(typeof(Extensions).Assembly);

        // Registrar API do módulo
        services.AddScoped<ISearchModuleApi, SearchModuleApi>();

        return services;
    }
}
