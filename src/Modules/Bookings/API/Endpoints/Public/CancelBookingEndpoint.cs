using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;
using MeAjudaAi.Modules.Bookings.Application.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class CancelBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}/cancel", CancelBookingAsync)
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CancelBooking")
        .WithSummary("Cancela um agendamento")
        .WithDescription("Cancela um agendamento existente.");
    }

    /// <summary>
    /// Cancela um agendamento existente caso este esteja em um estado cancelável.
    /// </summary>
    /// <param name="id">ID do agendamento a ser cancelado.</param>
    /// <param name="request">Requisição contendo o motivo do cancelamento.</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="authResolver">Resolvedor de autorização do prestador.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado 204 se cancelado com sucesso.</returns>
    private static async Task<IResult> CancelBookingAsync(
        Guid id,
        CancelBookingRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        [FromServices] ProviderAuthorizationResolver authResolver,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var authResult = await authResolver.ResolveAsync(context.User, cancellationToken);
        if (authResult.FailureKind is not EAuthorizationFailureKind.None and not EAuthorizationFailureKind.NotLinked)
        {
            var error = authResult.ToProblemResult();
            if (error != null) return error;
        }

        var correlationId = CorrelationHelper.ParseCorrelationId(context);

        var command = new CancelBookingCommand(
            id, 
            request.Reason, 
            authResult.IsAdmin, 
            authResult.ProviderId, 
            authResult.UserId, 
            correlationId);
        
        var result = await dispatcher.SendAsync<CancelBookingCommand, Result>(command, cancellationToken);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
        );
    }
}
