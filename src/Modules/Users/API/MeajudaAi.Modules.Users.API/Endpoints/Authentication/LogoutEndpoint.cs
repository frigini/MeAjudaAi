using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Authentication;

public class LogoutEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/auth/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("User logout")
            .WithDescription("Invalidates the user's refresh token")
            .RequireAuthorization()
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAsync(request, cancellationToken);
        return Ok(result);
    }
}