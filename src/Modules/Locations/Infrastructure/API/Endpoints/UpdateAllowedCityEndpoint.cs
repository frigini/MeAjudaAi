using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.Infrastructure.API.Endpoints;

/// <summary>
/// Endpoint para atualizar cidade permitida existente (Admin only)
/// </summary>
public class UpdateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/api/v1/admin/allowed-cities/{id:guid}", UpdateAsync)
            .WithName("UpdateAllowedCity")
            .WithSummary("Update allowed city")
            .WithDescription("Updates an existing allowed city")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAdmin();

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateAllowedCityRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAllowedCityCommand(
            id,
            request.CityName,
            request.StateSigla,
            request.IbgeCode,
            request.IsActive);

        var result = await commandDispatcher.DispatchAsync(command, cancellationToken);

        return result.Match(
            success => Results.Ok(Response.Success(success)),
            errors => HandleErrors(errors));
    }
}

/// <summary>
/// Request DTO para atualização de cidade permitida
/// </summary>
public sealed record UpdateAllowedCityRequest(
    string CityName,
    string StateSigla,
    int? IbgeCode,
    bool IsActive);
