using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using FluentValidation;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public static class ProviderAuthorizationResultExtensions
{
    public static IResult? ToProblemResult(this ProviderAuthorizationResult result)
    {
        return result.FailureKind switch
        {
            AuthorizationFailureKind.UpstreamFailure => 
                Results.Problem(result.ErrorMessage, statusCode: result.ErrorStatusCode ?? StatusCodes.Status500InternalServerError),
            AuthorizationFailureKind.Unauthorized => 
                Results.Problem(result.ErrorMessage ?? "Acesso não autorizado.", statusCode: StatusCodes.Status401Unauthorized),
            AuthorizationFailureKind.NotLinked => 
                Results.Problem("Usuário não possui prestador vinculado.", statusCode: StatusCodes.Status404NotFound),
            _ => null
        };
    }
}

public class SetProviderScheduleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedule", async (
            SetProviderScheduleRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] ProviderAuthorizationResolver authResolver,
            [FromServices] IValidator<SetProviderScheduleRequest> validator,
            [FromServices] ILogger<SetProviderScheduleEndpoint> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (request == null)
            {
                return Results.Problem("Corpo da requisição é obrigatório.", statusCode: StatusCodes.Status400BadRequest);
            }

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
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
                var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Results.Problem("Identificador do administrador não encontrado no token.", statusCode: StatusCodes.Status400BadRequest);
                }
                logger.LogInformation("Admin {AdminId} is setting schedule for Provider {ProviderId}", userIdClaim, targetProviderId);
            }
            else
            {
                targetProviderId = authResult.ProviderId!.Value;

                if (request.ProviderId != Guid.Empty && request.ProviderId != targetProviderId)
                {
                    return Results.Problem("O ProviderId informado não coincide com o prestador autenticado.", statusCode: StatusCodes.Status400BadRequest);
                }

                logger.LogInformation("Provider {ProviderId} is setting own schedule", targetProviderId);
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
            Guid correlationId;

            if (!string.IsNullOrEmpty(correlationIdHeader) && Guid.TryParse(correlationIdHeader, out var parsedId))
            {
                correlationId = parsedId;
            }
            else
            {
                if (!string.IsNullOrEmpty(correlationIdHeader))
                {
                    var maskedHeader = correlationIdHeader.Length > 4 
                        ? $"...{correlationIdHeader[^4..]}" 
                        : correlationIdHeader;
                    logger.LogDebug("Invalid X-Correlation-Id header received: {CorrelationId}. Generating new one.", maskedHeader);
                }
                correlationId = Guid.NewGuid();
            }

            var command = new SetProviderScheduleCommand(
                targetProviderId,
                request.Availabilities,
                correlationId);

            var result = await dispatcher.SendAsync<SetProviderScheduleCommand, Result>(command, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
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
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}