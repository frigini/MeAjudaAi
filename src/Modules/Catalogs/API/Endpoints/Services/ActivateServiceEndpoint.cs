using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.Services;

public class ActivateServiceEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateService")
            .WithSummary("Ativar serviço")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .RequireAuthorization("Admin");

    private static async Task<IResult> ActivateAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new ActivateServiceCommand(id);
        var result = await commandDispatcher.SendAsync<ActivateServiceCommand, Result>(command, cancellationToken);
        return Handle(result);
    }
}
