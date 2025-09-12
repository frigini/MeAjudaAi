using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Users.API.Endpoints;

public static class UsersModuleEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("/api/v1/users")
            .WithTags("Users")
            .RequireAuthorization(); // Base auth requirement

        endpoints.MapEndpoint<CreateUserEndpoint>()
            .MapEndpoint<DeleteUserEndpoint>()
            .MapEndpoint<GetUserByEmailEndpoint>()
            .MapEndpoint<GetUserByIdEndpoint>()
            .MapEndpoint<GetUsersEndpoint>()
            .MapEndpoint<UpdateUserProfileEndpoint>();
    }
}