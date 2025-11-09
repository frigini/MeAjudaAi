using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Handlers.Commands;
using MeAjudaAi.Modules.Users.Application.Handlers.Queries;
using MeAjudaAi.Modules.Users.Application.ModuleApi;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Services;
using MeAjudaAi.Modules.Users.Application.Services.Implementations;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Users.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Query Handlers - registro manual para garantir disponibilidade
        services.AddScoped<IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>, GetUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByIdQuery, Result<UserDto>>, GetUserByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByEmailQuery, Result<UserDto>>, GetUserByEmailQueryHandler>();
        services.AddScoped<IQueryHandler<GetUserByUsernameQuery, Result<UserDto>>, GetUserByUsernameQueryHandler>();
        services.AddScoped<IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>>, GetUsersByIdsQueryHandler>();

        // Command Handlers - registro manual para garantir disponibilidade  
        services.AddScoped<ICommandHandler<CreateUserCommand, Result<UserDto>>, CreateUserCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateUserProfileCommand, Result<UserDto>>, UpdateUserProfileCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteUserCommand, Result>, DeleteUserCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeUserEmailCommand, Result<UserDto>>, ChangeUserEmailCommandHandler>();
        services.AddScoped<ICommandHandler<ChangeUserUsernameCommand, Result<UserDto>>, ChangeUserUsernameCommandHandler>();

        // Cache Services específicos do módulo
        services.AddScoped<IUsersCacheService, UsersCacheService>();

        // Module API - interface pública para outros módulos
        services.AddScoped<IUsersModuleApi, UsersModuleApi>();

        return services;
    }
}
