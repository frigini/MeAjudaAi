using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class UpdateUserEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/api/v1/users/{id:guid}", UpdateUserAsync)
            .WithName("UpdateUser")
            .WithSummary("Update user")
            .WithDescription("Updates an existing user's information")
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserRequest request,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(result);
    }
}