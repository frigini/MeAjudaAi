using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

public class UpdateMyProviderProfileEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("api/v1/providers/me", UpdateMyProfileAsync)
            .WithName("UpdateMyProviderProfile")
            .WithTags("Providers - Me")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> UpdateMyProfileAsync(
        HttpContext context,
        [FromBody] UpdateProviderProfileRequest request,
        IQueryDispatcher queryDispatcher,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Request body is required");

        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Invalid User ID format");

        // First, get the provider ID for the current user
        var query = new GetProviderByUserIdQuery(userId);
        var providerResult = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (providerResult.IsFailure || providerResult.Value is null)
        {
            return NotFound("Provider profile not found for the current user.");
        }

        var providerId = providerResult.Value.Id;

        // Dispatch update command using the found provider ID
        var command = request.ToCommand(providerId);
        var result = await commandDispatcher.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
