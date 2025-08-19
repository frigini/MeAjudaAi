using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class DeleteUserEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/api/v1/users/{id:guid}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Delete user")
            .WithDescription("Removes a user from the system")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.DeleteUserAsync(id, cancellationToken);
        return NoContent(result);
    }
}