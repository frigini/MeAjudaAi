using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class ConfirmBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}/confirm", async (
            Guid id,
            [FromServices] ICommandDispatcher dispatcher,
            ClaimsPrincipal user,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              user.FindFirst(AuthConstants.Claims.Subject)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Forbid();
            }

            var correlationIdHeader = context.Request.Headers["X-Correlation-Id"].ToString();
            if (!Guid.TryParse(correlationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid();
            }

            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);
            var providerIdClaimValue = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            Guid? userProviderId = Guid.TryParse(providerIdClaimValue, out var parsedProviderId) ? parsedProviderId : null;

            var command = new ConfirmBookingCommand(id, userId, isSystemAdmin, userProviderId, correlationId);
            var result = await dispatcher.SendAsync<ConfirmBookingCommand, Result>(command, cancellationToken);

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
        .WithTags(BookingsEndpoints.Tag)
        .WithName("ConfirmBooking")
        .WithSummary("Confirma um agendamento pendente.");
    }
}
