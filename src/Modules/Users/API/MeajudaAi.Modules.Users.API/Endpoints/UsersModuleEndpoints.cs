using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Modules.Users.API.Endpoints;

public static class UsersModuleEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        // Use the unified versioning system via BaseEndpoint
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, "users", "Users")
            .RequireAuthorization(); // Apply global authorization

        endpoints.MapEndpoint<CreateUserEndpoint>()
            .MapEndpoint<DeleteUserEndpoint>()
            .MapEndpoint<GetUserByEmailEndpoint>()
            .MapEndpoint<GetUserByIdEndpoint>()
            .MapEndpoint<GetUsersEndpoint>()
            .MapEndpoint<UpdateUserProfileEndpoint>();
    }
}