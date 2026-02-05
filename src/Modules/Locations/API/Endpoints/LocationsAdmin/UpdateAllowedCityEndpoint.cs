using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using MeAjudaAi.Contracts.Models;
namespace MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;

/// <summary>
/// Endpoint para atualizar cidade permitida existente (Admin only)
/// </summary>
public class UpdateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("{id:guid}", UpdateAsync)
            .WithName("UpdateAllowedCity")
            .WithSummary("Atualizar cidade permitida")
            .WithDescription("Atualiza uma cidade permitida existente")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAdmin();

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateAllowedCityRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);

        var result = await commandDispatcher.SendAsync<UpdateAllowedCityCommand, Result>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            // Mapeia o código de erro/status para uma mensagem localizada segura
            var errorMessage = result.Error.StatusCode switch
            {
                StatusCodes.Status409Conflict => ValidationMessages.Locations.DuplicateCity,
                StatusCodes.Status404NotFound => ValidationMessages.Locations.AllowedCityNotFound,
                _ => ValidationMessages.Locations.UpdateFailed 
            };

            return Results.Problem(
                detail: errorMessage,
                statusCode: result.Error.StatusCode,
                title: "Erro ao atualizar cidade permitida");
        }

        return Results.NoContent();
    }
}
