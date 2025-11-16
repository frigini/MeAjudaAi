using FluentValidation;
using MeAjudaAi.Modules.Search.Application.DTOs;
using MeAjudaAi.Modules.Search.Application.Queries;
using MeAjudaAi.Modules.Search.Application.Services;
using MeAjudaAi.Shared.Contracts.Modules.Search;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Search.Application;

/// <summary>
/// Extension methods for registering Search Application layer services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers Search Application layer services.
    /// </summary>
    public static IServiceCollection AddSearchApplication(this IServiceCollection services)
    {
        // Register query handlers
        services.AddScoped<IQueryHandler<SearchProvidersQuery, Result<PagedSearchResultDto<SearchableProviderDto>>>, Handlers.SearchProvidersQueryHandler>();

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(typeof(Extensions).Assembly);

        // Register module API
        services.AddScoped<ISearchModuleApi, SearchModuleApi>();

        return services;
    }
}
