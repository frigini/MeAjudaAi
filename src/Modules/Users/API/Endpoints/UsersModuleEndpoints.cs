using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;
using MeAjudaAi.Modules.Users.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.Users.API.Endpoints;

public static class UsersModuleEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        MapUsersEndpoints(app);
    }

    public static void MapUsersEndpoints(IEndpointRouteBuilder app)
    {
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Users.Base, ModuleNames.Users)
            .RequireAuthorization();

        endpoints.MapEndpoint<CreateUserEndpoint>()
            .MapEndpoint<DeleteUserEndpoint>()
            .MapEndpoint<GetUserByEmailEndpoint>()
            .MapEndpoint<GetUserByIdEndpoint>()
            .MapEndpoint<GetUsersEndpoint>()
            .MapEndpoint<UpdateUserProfileEndpoint>()
            .MapEndpoint<RegisterCustomerEndpoint>()
            .MapEndpoint<UpdateUserDeviceTokenEndpoint>()
            .MapEndpoint<GetAuthProvidersEndpoint>();
    }
}
