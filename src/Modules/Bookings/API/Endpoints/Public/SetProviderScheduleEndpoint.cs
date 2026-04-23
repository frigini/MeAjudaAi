using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class SetProviderScheduleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedule", async (
            SetProviderScheduleRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (request == null || request.Availabilities == null)
            {
                return Results.Problem("Corpo da requisição ou disponibilidades ausentes.", statusCode: StatusCodes.Status400BadRequest);
            }

            var user = context.User;
            // Verifica tanto o claim proprietário (sub) quanto o padrão do ASP.NET (NameIdentifier)
            var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value
                           ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

            Guid targetProviderId;

            if (isSystemAdmin)
            {
                targetProviderId = request.ProviderId;
                var logger = context.RequestServices.GetRequiredService<ILogger<SetProviderScheduleEndpoint>>();
                logger.LogInformation("Admin {AdminId} is setting schedule for Provider {ProviderId}", userIdClaim, targetProviderId);
            }
            else if (!string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var pId))
            {
                targetProviderId = pId;
            }
            else if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var uId))
            {
                // Tenta resolver o ProviderId pelo UserId se o claim de provider não estiver presente
                var providerResult = await providersApi.GetProviderByUserIdAsync(uId, cancellationToken);
                
                if (providerResult.IsFailure)
                {
                    return Results.Problem(providerResult.Error.Message, statusCode: providerResult.Error.StatusCode);
                }

                if (providerResult.Value == null)
                {
                    return Results.Forbid();
                }

                targetProviderId = providerResult.Value.Id;
            }
            else
            {
                return Results.Unauthorized();
            }

            if (targetProviderId == Guid.Empty)
            {
                return Results.Problem("ProviderId inválido ou ausente.", statusCode: StatusCodes.Status400BadRequest);
            }

            // Para não-admins, valida se o ProviderId no request coincide (se enviado)
            if (!isSystemAdmin && request.ProviderId != Guid.Empty && request.ProviderId != targetProviderId)
            {
                return Results.Problem("O ProviderId informado não coincide com o prestador autenticado.", statusCode: StatusCodes.Status400BadRequest);
            }

            // Resolve Correlation ID
            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].ToString();
            if (!Guid.TryParse(correlationIdHeader, out var correlationId))
            {
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
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)

        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}

/// <summary>
/// Requisito para definição de agenda.
/// </summary>
/// <param name="ProviderId">ID do prestador. Honrado apenas se o solicitante for IsSystemAdmin.</param>
/// <param name="Availabilities">Lista de disponibilidades por dia da semana.</param>
public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<ProviderScheduleDto> Availabilities);
