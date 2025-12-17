using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;
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
            .WithSummary("Criar nova cidade permitida")
            .WithDescription("Cria uma nova cidade permitida para operações de prestadores (apenas Admin)")
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
