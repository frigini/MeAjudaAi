using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

public class UpdateUserProfileEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}/profile", UpdateUserAsync)
            .WithName("UpdateUserProfile")
            .WithSummary("Update user")
            .WithDescription("Updates an existing user's information")
            .RequireAuthorization("SelfOrAdmin")
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserProfileRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserProfileCommand(id, request.FirstName, request.LastName, request.Email);
        var result = await commandDispatcher.SendAsync<UpdateUserProfileCommand, Result<UserDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}