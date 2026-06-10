using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.Authorization;
using MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public sealed class SetProviderScheduleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Bookings.SetProviderSchedule, SetProviderScheduleAsync)
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status502BadGateway)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
        .ProducesProblem(StatusCodes.Status504GatewayTimeout)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define agenda")
        .WithDescription("Define a agenda de horários de trabalho de um prestador.");
    }

    /// <summary>
    /// Define a grade de horários de disponibilidade de um prestador.
    /// </summary>
    /// <param name="request">Dados da nova grade de horários.</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="providersApi">Interface de integração com módulo de prestadores.</param>
    /// <param name="authResolver">Resolvedor de autorização do prestador.</param>
    /// <param name="logger">Logger da aplicação.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado 204 se a agenda for definida com sucesso.</returns>
    private static async Task<IResult> SetProviderScheduleAsync(
        SetProviderScheduleRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        [FromServices] IProvidersModuleApi providersApi,
        [FromServices] ProviderAuthorizationResolver authResolver,
        [FromServices] ILogger<SetProviderScheduleEndpoint> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return Results.Problem("Corpo da requisição é obrigatório.", statusCode: StatusCodes.Status400BadRequest);
        }

        var authResult = await authResolver.ResolveAsync(context.User, cancellationToken);

        var authError = authResult.ToProblemResult();
        if (authError != null)
        {
            return authError;
        }

        Guid targetProviderId;

        if (authResult.IsAdmin)
        {
            if (request.ProviderId == Guid.Empty)
            {
                return Results.Problem("ProviderId inválido para operação admin.", statusCode: StatusCodes.Status400BadRequest);
            }
            targetProviderId = request.ProviderId;
            var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value
                ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Results.Problem("Identificador do administrador não encontrado no token.", statusCode: StatusCodes.Status400BadRequest);
            }
            logger.LogInformation("Admin {AdminId} is setting schedule for Provider {ProviderId}", userIdClaim, targetProviderId);
        }
        else
        {
            if (!authResult.ProviderId.HasValue)
            {
                logger.LogError("Authorization resolver did not set ProviderId for non-admin user {UserId}", authResult.UserId);
                return Results.Problem("Erro interno de configuração: identificador do prestador não encontrado.", statusCode: StatusCodes.Status500InternalServerError);
            }

            targetProviderId = authResult.ProviderId.Value;

            if (request.ProviderId != Guid.Empty && request.ProviderId != targetProviderId)
            {
                return Results.Problem("O ProviderId informado não coincide com o prestador autenticado.", statusCode: StatusCodes.Status400BadRequest);
            }

            logger.LogInformation("Provider {ProviderId} is setting own schedule", targetProviderId);
        }

        var correlationId = CorrelationHelper.ParseCorrelationId(context);

        var command = new SetProviderScheduleCommand(
            targetProviderId,
            request.Availabilities,
            correlationId);

        var result = await dispatcher.SendAsync<SetProviderScheduleCommand, Result>(command, cancellationToken);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
        );
    }
}
