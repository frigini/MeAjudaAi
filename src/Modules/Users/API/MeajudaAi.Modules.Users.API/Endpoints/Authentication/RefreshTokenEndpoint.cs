using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Authentication;

public class RefreshTokenEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/auth/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .WithDescription("Generates a new access token using a valid refresh token")
            .AllowAnonymous()
            .Produces<Response<AuthResponseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(result);
    }
}