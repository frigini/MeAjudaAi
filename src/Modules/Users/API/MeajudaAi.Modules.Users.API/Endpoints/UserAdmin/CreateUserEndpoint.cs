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

public class CreateUserEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/users", CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Create new user")
            .WithDescription("Creates a new user in the system")
            .Produces<Response<UserDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        IUserManagementService userService,
        CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(request, cancellationToken);
        return Created(result, "GetUser", new { id = result.Value?.Id });
    }
}