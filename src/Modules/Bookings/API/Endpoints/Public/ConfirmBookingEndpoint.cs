using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Common;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class ConfirmBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}/confirm", async (
            Guid id,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] ProviderAuthorizationResolver authResolver,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var authResult = await authResolver.ResolveAsync(context.User, cancellationToken);
            if (authResult.FailureKind != AuthorizationFailureKind.None)
            {
                var error = authResult.ToProblemResult();
                if (error != null) return error;
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].ToString();
            if (!Guid.TryParse(correlationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid();
            }

            var command = new ConfirmBookingCommand(
                id, 
                authResult.IsAdmin, 
                authResult.ProviderId, 
                correlationId);
            
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
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("ConfirmBooking")
        .WithSummary("Confirma um agendamento pendente.");
    }
}
