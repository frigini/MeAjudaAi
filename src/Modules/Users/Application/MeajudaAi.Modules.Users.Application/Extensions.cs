using MeAjudaAi.Modules.Users.Application.Caching;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // REMOVED: Command/Query Handlers são registrados automaticamente pelo Scrutor no Shared
        // O Scrutor já faz isso através de:
        // - services.Scan(...).AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
        // - services.Scan(...).AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))

        // Cache Services específicos do módulo
        services.AddScoped<IUsersCacheService, UsersCacheService>();

        return services;
    }
}