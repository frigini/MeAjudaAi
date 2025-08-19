using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Authentication;

public class LoginEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/auth/login", LoginAsync)
            .WithName("Login")
            .WithSummary("User login")
            .WithDescription("Authenticates a user and returns access and refresh tokens")
            .AllowAnonymous()
            .Produces<Response<AuthResponseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);


    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }
}