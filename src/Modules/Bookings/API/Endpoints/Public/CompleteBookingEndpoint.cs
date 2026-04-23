using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
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
        app.MapPut("/{id}/complete", async (
            Guid id,
            [FromServices] ICommandDispatcher dispatcher,
            System.Security.Claims.ClaimsPrincipal user,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
            var correlationId = Guid.TryParse(correlationIdHeader, out var cId) ? cId : Guid.NewGuid();

            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);
            var providerIdClaimValue = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            Guid? userProviderId = Guid.TryParse(providerIdClaimValue, out var parsedProviderId) ? parsedProviderId : null;

            var command = new CompleteBookingCommand(id, isSystemAdmin, userProviderId, correlationId);
            var result = await dispatcher.SendAsync<CompleteBookingCommand, Result>(command, cancellationToken);

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
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CompleteBooking")
        .WithSummary("Marca um agendamento confirmado como concluído.");
    }
}
