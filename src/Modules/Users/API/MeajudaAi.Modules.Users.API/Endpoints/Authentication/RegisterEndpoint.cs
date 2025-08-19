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

public class RegisterEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/auth/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("User registration")
            .WithDescription("Creates a new user account")
            .AllowAnonymous()
            .Produces<Response<AuthResponseDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterRequest request,
        IAuthenticationService authService,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        return Created(result, "GetUser", new { id = result.Value?.User?.Id });
    }
}