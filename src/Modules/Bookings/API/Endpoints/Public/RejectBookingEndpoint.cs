using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
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
    /// Rejeita um agendamento pendente.
    /// </summary>
    /// <param name="id">ID do agendamento a ser rejeitado.</param>
    /// <param name="request">Dados da rejeição (motivo).</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado 204 se rejeitado com sucesso.</returns>
    private static async Task<IResult> RejectBookingAsync(
        Guid id,
        RejectBookingRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var user = context.User;
        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);
        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        Guid? userProviderId = Guid.TryParse(providerIdClaim, out var pId) ? pId : null;

        var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
        var correlationId = Guid.TryParse(correlationIdHeader, out var cId) ? cId : Guid.NewGuid();

        var command = new RejectBookingCommand(id, request.Reason, isSystemAdmin, userProviderId, correlationId);
        var result = await dispatcher.SendAsync<RejectBookingCommand, Result>(command, cancellationToken);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
        );
    }
}
