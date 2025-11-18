using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.Services;

public class DeactivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateService")
            .WithSummary("Desativar serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> DeactivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateServiceCommand(id);
        var result = await commandDispatcher.SendAsync<DeactivateServiceCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}
