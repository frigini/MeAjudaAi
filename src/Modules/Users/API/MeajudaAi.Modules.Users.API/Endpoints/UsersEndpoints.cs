using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Interfaces;
using MeAjudaAi.Shared.Common;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints;

public class UsersEndpoints : BaseEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = CreateGroup(app, "auth", "Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("Login")
            .WithSummary("User login")
            .Produces<Response<AuthResponseDto>>(200)
            .Produces<Response<object>>(400);

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("User registration")
            .Produces<Response<AuthResponseDto>>(201)
            .Produces<Response<object>>(400);

        group.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token")
            .Produces<Response<AuthResponseDto>>(200)
            .Produces<Response<object>>(400);

        group.MapPost("/logout", LogoutAsync)
            .WithName("Logout")
            .WithSummary("User logout")
            .Produces<Response<object>>(200)
            .Produces<Response<object>>(400);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Created(result, "GetUser", new { id = result.Value?.User.Id });
    }

    private static async Task<IResult> RefreshTokenAsync(
        RefreshTokenRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request, cancellationToken);
        return Ok(result);
    }

    private static async Task<IResult> LogoutAsync(
        LogoutRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAsync(request, cancellationToken);
        return Ok(result);
    }
}
