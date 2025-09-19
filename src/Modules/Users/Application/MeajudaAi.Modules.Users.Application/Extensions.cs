using MeAjudaAi.Modules.Users.Application.Caching;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Command Handlers
        services.AddScoped<ICommandHandler<CreateUserCommand, Result<UserDto>>, CreateUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateUserProfileCommand, Result<UserDto>>, UpdateUserProfileCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand, Result>, DeleteUserCommandHandler>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetUserByIdQuery, Result<UserDto>>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>, GetUserByEmailQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>, GetUsersQueryHandler>();

        // Cache Services
        services.AddScoped<IUsersCacheService, UsersCacheService>();

        return services;
    }
}