using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class CompleteBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Bookings.Complete, CompleteBookingAsync)
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CompleteBooking")
        .WithSummary("Completa um agendamento")
        .WithDescription("Marca um agendamento confirmado como concluído.");
    }

    /// <summary>
    /// Finaliza um agendamento, alterando seu status para concluído.
    /// </summary>
    /// <param name="id">ID do agendamento a ser finalizado.</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="authResolver">Resolvedor de autorização do prestador.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado 204 se concluído com sucesso.</returns>
    private static async Task<IResult> CompleteBookingAsync(
        Guid id,
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

        var command = new CompleteBookingCommand(id, authResult.IsAdmin, authResult.ProviderId, correlationId);
        var result = await dispatcher.SendAsync<CompleteBookingCommand, Result>(command, cancellationToken);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
        );
    }
}
