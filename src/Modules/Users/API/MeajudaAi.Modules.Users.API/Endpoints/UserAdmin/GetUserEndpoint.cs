using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class GetUserEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/api/v1/users/{id:guid}", GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Get user by ID")
            .WithDescription("Retrieves a specific user by their unique identifier")
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetUserAsync(
        Guid id,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.GetUserByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}