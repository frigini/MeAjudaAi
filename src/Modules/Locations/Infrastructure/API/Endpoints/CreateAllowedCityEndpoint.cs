using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.Infrastructure.API.Endpoints;

/// <summary>
/// Endpoint para criar nova cidade permitida (Admin only)
/// </summary>
public class CreateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/api/v1/admin/allowed-cities", CreateAsync)
            .WithName("CreateAllowedCity")
            .WithSummary("Create new allowed city")
            .WithDescription("Creates a new allowed city for provider operations (Admin only)")
            .Produces<Response<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAdmin();

    private static async Task<IResult> CreateAsync(
        CreateAllowedCityRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new CreateAllowedCityCommand(
            request.CityName,
            request.StateSigla,
            request.IbgeCode,
            request.IsActive);

        var cityId = await commandDispatcher.SendAsync<CreateAllowedCityCommand, Guid>(command, cancellationToken);

        return Results.Created($"/api/v1/admin/allowed-cities/{cityId}", new Response<Guid>(cityId, 201));
    }
}

/// <summary>
/// Request DTO para criação de cidade permitida
/// </summary>
public sealed record CreateAllowedCityRequest(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive = true);
