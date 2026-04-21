using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class SetProviderScheduleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedule", async (
            SetProviderScheduleRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

            if (!isSystemAdmin && (string.IsNullOrEmpty(providerIdClaim) || !Guid.TryParse(providerIdClaim, out _)))
            {
                return Results.Forbid();
            }

            // Se for admin, pode usar o ID do corpo. Se for prestador, usa o ID do claim.
            var targetProviderId = isSystemAdmin 
                ? request.ProviderId 
                : Guid.Parse(providerIdClaim!);

            if (targetProviderId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "ProviderId inválido ou ausente." });
            }

            var command = new SetProviderScheduleCommand(
                targetProviderId,
                request.Availabilities);

            var result = await dispatcher.SendAsync<SetProviderScheduleCommand, Result>(command, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}

public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<AvailabilityDto> Availabilities);
