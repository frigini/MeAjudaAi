using MeAjudaAi.Modules.Users.API.Endpoints.Authentication;
using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Modules.Users.API.Endpoints;

public static class UsersModuleEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var endpoints = app.MapGroup("");

        endpoints.MapGroup("v1/users")
            .WithTags("Users")
            .MapEndpoint<GetUsersEndpoint>()
            .MapEndpoint<GetUserEndpoint>()
            .MapEndpoint<CreateUserEndpoint>()
            .MapEndpoint<UpdateUserEndpoint>()
            .MapEndpoint<DeleteUserEndpoint>();

        endpoints.MapGroup("v1/auth")
            .WithTags("Authentication")
            .MapEndpoint<LoginEndpoint>()
            .MapEndpoint<RegisterEndpoint>()
            .MapEndpoint<RefreshTokenEndpoint>()
            .MapEndpoint<LogoutEndpoint>();
    }
}