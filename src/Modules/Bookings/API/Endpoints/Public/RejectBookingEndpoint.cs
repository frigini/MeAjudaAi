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

public class RejectBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}/reject", RejectBookingAsync)
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("RejectBooking")
        .WithSummary("Rejeita um agendamento")
        .WithDescription("Rejeita um agendamento pendente.");
    }

    /// <summary>
    /// Rejeita um agendamento pendente de aprovação.
    /// </summary>
    /// <param name="id">O identificador único do agendamento a ser rejeitado.</param>
    /// <param name="request">O objeto contendo os detalhes da rejeição, como o motivo.</param>
    /// <param name="dispatcher">O despachante de comandos para processar a rejeição.</param>
    /// <param name="authResolver">O resolvedor de autorização do prestador.</param>
    /// <param name="context">O contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Retorna 204 No Content se a rejeição for bem-sucedida, ou um problema detalhado caso contrário.</returns>
    private static async Task<IResult> RejectBookingAsync(
        Guid id,
        RejectBookingRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        [FromServices] ProviderAuthorizationResolver authResolver,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var authResult = await authResolver.ResolveAsync(context.User, cancellationToken);
        if (authResult.FailureKind != EAuthorizationFailureKind.None)
        {
            var error = authResult.ToProblemResult();
            if (error != null) return error;
        }

        var correlationId = CorrelationHelper.ParseCorrelationId(context);

        var command = new RejectBookingCommand(id, request.Reason, authResult.IsAdmin, authResult.ProviderId, correlationId);
        
        var result = await dispatcher.SendAsync<RejectBookingCommand, Result>(command, cancellationToken);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
        );
    }
}
