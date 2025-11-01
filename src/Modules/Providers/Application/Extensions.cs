using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Contracts;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Providers.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Manual registration for GetProvidersQuery to test if it's assembly loading issue
        services.AddScoped<IQueryHandler<GetProvidersQuery, Result<PagedResult<ProviderDto>>>, GetProvidersQueryHandler>();

        // Module API - registro da API pública para comunicação entre módulos
        services.AddScoped<IProvidersModuleApi, ProvidersModuleApi>();

        return services;
    }
}
